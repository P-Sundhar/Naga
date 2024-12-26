using Syncfusion.SfDataGrid.XForms;
using Xamarin.Forms;

namespace StockAudit
{
    public class CustomStyle : DataGridStyle
    {
        public CustomStyle()
        {
        }
        public override Color GetHeaderBackgroundColor()
        {
            return Color.FromRgb(0, 96, 173);
        }

        public override Color GetHeaderForegroundColor()
        {
            return Color.FromRgb(255, 255, 255);
        }

        public override Color GetRecordBackgroundColor()
        {
            return Color.LightBlue;
        }

        public override Color GetRecordForegroundColor()
        {
            return Color.FromRgb(0, 0, 0);
        }

        public override Color GetSelectionBackgroundColor()
        {
            return Color.FromRgb(42, 159, 214);
        }

        public override Color GetSelectionForegroundColor()
        {
            return Color.FromRgb(255, 255, 255);
        }

        public override Color GetCaptionSummaryRowBackgroundColor()
        {
            return Color.FromRgb(02, 02, 02);
        }

        public override Color GetCaptionSummaryRowForeGroundColor()
        {
            return Color.FromRgb(255, 255, 255);
        }

        public override Color GetBorderColor()
        {
            return Color.FromRgb(184, 173, 0);
        }

        public override Color GetHeaderBorderColor()
        {
            return Color.White;
        }

        public override Color GetLoadMoreViewBackgroundColor()
        {
            return Color.FromRgb(242, 242, 242);
        }

        public override Color GetLoadMoreViewForegroundColor()
        {
            return Color.FromRgb(34, 31, 31);
        }

        public override Color GetAlternatingRowBackgroundColor()
        {
            return Color.LightBlue;
        }
    }
}
