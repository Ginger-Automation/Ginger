namespace Ginger.Reports
{
    public interface IHTMLReportConfigurationOperations
    {
        bool CheckIsDefault();
        void SetHTMLReportConfigurationWithDefaultValues(HTMLReportConfiguration reportConfiguraion);
        int SetReportTemplateSequence(bool isAddTemplate);
    }
}
