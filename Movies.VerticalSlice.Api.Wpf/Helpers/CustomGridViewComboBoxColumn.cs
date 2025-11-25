using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Movies.VerticalSlice.Api.Wpf.Helpers
{
    public class CustomGridViewComboBoxColumn : GridViewComboBoxColumn
    {
        public override FrameworkElement CreateCellEditElement(GridViewCell cell, object dataItem)
        {
            var comboBox = (RadComboBox)base.CreateCellEditElement(cell, dataItem);
            comboBox.IsEditable = true;
            return comboBox;
        }
    }
}
