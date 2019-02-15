import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';


@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})


export class FetchDataComponent
{
  public forecasts: WeatherForecast[];
  public report: string;
  mHttp: HttpClient;
  mBaseUrl: string;

  
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string)
  {
    this.mHttp = http;
    this.mBaseUrl = baseUrl;

    http.get<WeatherForecast[]>(baseUrl + 'api/BusinessFlow/BusinessFlows').subscribe(result => {
      this.forecasts = result;
    }, error => console.error(error));

  }

  public run(BF:WeatherForecast) {

    BF.status = "Running";
    BF.elapsed = -1;
    const req = this.mHttp.post<RunBusinessFlowResult>(this.mBaseUrl + 'api/BusinessFlow/RunBusinessFlow', {
      name: BF.name  //TODO: We send the BF name replace with BF.Guid
    })
      .subscribe(
      res => {
        // Once we get the response        
        BF.status = res.status;
        BF.elapsed = res.elapsed;
        // this.report = res.report;
      },
        err => {
          console.log("Error occured");
          BF.status = "Error 123";
        }
      );
  }

  public flowReport(BF: WeatherForecast) {
    
  }



}



interface RunBusinessFlowResult {
  status: string;
  elapsed: number;
  report: string;
}

interface WeatherForecast {
  name: string;
  description: string;
  fileName: string;
  status: string;
  elapsed: number;  
}
