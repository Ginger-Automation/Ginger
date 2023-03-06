#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace VisualRegressionTracker
{
    public class VisualRegressionTracker
    {
        private class Disposer //: IAsyncDisposable 
        {
            private readonly VisualRegressionTracker tracker;
            private readonly CancellationToken? cancellationToken;

            public Disposer(VisualRegressionTracker tracker, CancellationToken? cancellationToken)
            {
                this.tracker = tracker;
                this.cancellationToken = cancellationToken;
            }

            //public ValueTask DisposeAsync()
            //{
            //    return tracker.Stop(cancellationToken);
            //}
        }

        private readonly Config config;
        private readonly ApiClient client;
        private string buildId;
        private string projectId;

        public VisualRegressionTracker() : this(null, null)
        {
        }

        public VisualRegressionTracker(Config config) : this(config, null) 
        {
        }

        public VisualRegressionTracker(Config config, HttpClient httpClient)
        {
            this.config = config ?? Config.GetDefault();
            this.config.CheckComplete();

            var apiUrl = this.config.ApiUrl + (this.config.ApiUrl.EndsWith("/") ? "" : "/");

            this.client = new ApiClient(
                apiUrl,
                httpClient ?? new HttpClient()
            )
            {
                ApiKey = this.config.ApiKey,
                Project = this.config.Project
            };
        }

        public bool IsStarted => buildId != null && projectId != null;
        public string BuildId => buildId;
        public string ProjectId => projectId;

        public Task Start()
        {
            return Start(null);
        }

        public Task Start(CancellationToken cancellationToken)
        {
            return Start((CancellationToken?) cancellationToken);
        }

        protected async Task Start(CancellationToken? cancellationToken)
        {
            try
            {
                var dto = new CreateBuildDto
                {
                    Project = config.Project,
                    BranchName = config.BranchName,
                    CiBuildId = config.CiBuildId
                };
                var response = cancellationToken.HasValue
                    ? await client.BuildsController_createAsync(dto, cancellationToken.Value)
                    : await client.BuildsController_createAsync(dto);

                buildId = response.Id;
                projectId = response.ProjectId;
                //return new Disposer(this, cancellationToken);
            }
            catch (ApiException ex)
            {
                throw new VisualRegressionTrackerError(ex);
            }
            catch (HttpRequestException ex)
            {
                throw new VisualRegressionTrackerError(ex);
            }
        }
         
        public Task Stop()
        {
            return Stop(null);
        }
         
        public Task Stop(CancellationToken cancellationToken)
        {
            return Stop((CancellationToken?) cancellationToken);
        }

        protected async Task Stop(CancellationToken? cancellationToken)
        {
            if (!IsStarted) {
                throw new VisualRegressionTrackerError("Visual Regression Tracker has not been started");
            }

            try
            {
                if (cancellationToken.HasValue) {
                    await client.BuildsController_stopAsync(buildId, cancellationToken.Value);
                } else {
                    await client.BuildsController_stopAsync(buildId);
                }

                buildId = null;
                projectId = null;
            }
            catch (ApiException ex)
            {
                throw new VisualRegressionTrackerError(ex);
            }
            catch (HttpRequestException ex)
            {
                throw new VisualRegressionTrackerError(ex);
            }
        }

        protected async Task<TestRunResult> SubmitTestRun(
            CreateTestRequestDto testRun, CancellationToken? cancellationToken)
        {
            if (!IsStarted) {
                throw new VisualRegressionTrackerError("Visual Regression Tracker has not been started");
            }

            var response = cancellationToken.HasValue
                ? await client.TestRunsController_postTestRunAsync(testRun, cancellationToken.Value)
                : await client.TestRunsController_postTestRunAsync(testRun);
            var status = TestRunStatus.New;
            switch(response.Status)
            {
                case "new":
                    status = TestRunStatus.New;
                    break;
                case "ok":
                    status = TestRunStatus.Ok;
                    break;

                case "unresolved":
                    status = TestRunStatus.Unresolved;
                    break;
                case "failed":
                    status = TestRunStatus.Failed;
                    break;
                case "approved":
                    status = TestRunStatus.Approved;
                    break;
                case "autoApproved":
                    status = TestRunStatus.AutoApproved;
                    break;
                default:
                    throw new VisualRegressionTrackerError("Unexpected status");
                    break;
            }

            var result = new TestRunResult
            {
                Status = status,
                Url = response.Url,
                ImageUrl = $"{response.Url}/{response.ImageName}",
                BaselineUrl = !string.IsNullOrEmpty(response.BaselineName)
                    ? $"{response.Url}/{response.BaselineName}"
                    : null,
                DiffUrl = !string.IsNullOrEmpty(response.DiffName)
                    ? $"{response.Url}/{response.DiffName}"
                    : null,
            };

            return result;
        }

        public async Task<TestRunResult> Track(
            string name,
            string imageBase64,
            CancellationToken? cancellationToken = null,
            string os = null,
            string browser = null,
            string viewport = null,
            string device = null,
            string customTags = null,
            double? diffTollerancePercent = null,
            IEnumerable<IgnoreAreaDto> ignoreAreas = null)
        {
            var dto = new CreateTestRequestDto
            {
                BuildId = BuildId,
                ProjectId = ProjectId,
                BranchName = config.BranchName,
                Name = name,
                ImageBase64 = imageBase64,
                Os = os,
                Browser = browser,
                Viewport = viewport,
                Device = device,
                CustomTags = customTags,
                DiffTollerancePercent = diffTollerancePercent ?? 0,
                IgnoreAreas = ignoreAreas == null ? null : new List<IgnoreAreaDto>(ignoreAreas),
            };

            return await SubmitTestRun(dto, cancellationToken).ConfigureAwait(false);
        }
 
        public Task<TestRunResult> Track(
            string name,
            Stream image,
            CancellationToken? cancellationToken = null,
            string os = null,
            string browser = null,
            string viewport = null,
            string device = null,
            string customTags = null,
            double? diffTollerancePercent = null,
            IEnumerable<IgnoreAreaDto> ignoreAreas = null)
        {
            using (var base64Stream = new CryptoStream(image, new ToBase64Transform(), CryptoStreamMode.Read))
            {
                using (var reader = new StreamReader(base64Stream))
                {
                    var imageBase64 = reader.ReadToEnd();
                    return Track(
                        name,
                        imageBase64,
                        cancellationToken,
                        os, browser, viewport, device, customTags, diffTollerancePercent, ignoreAreas
                    );
                }
            }
        }

        public Task<TestRunResult> Track(
            string name,
            byte[] image,
            CancellationToken? cancellationToken = null,
            string os = null,
            string browser = null,
            string viewport = null,
            string device = null,
            string customTags = null,
            double? diffTollerancePercent = null,
            IEnumerable<IgnoreAreaDto> ignoreAreas = null)
        {
            using (var memoryStream = new MemoryStream(image))
            {
                return Track(
                    name,
                    memoryStream,
                    cancellationToken,
                    os, browser, viewport, device, customTags, diffTollerancePercent, ignoreAreas
                );
            }
        }
    }
}