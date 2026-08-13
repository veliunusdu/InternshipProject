using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;

namespace Project1.Module.Controllers.Customers
{
    /// <summary>
    /// Popup pencerelerinin oluşturulması ve kaydedilmesi işlemlerindeki ortak kod tekrarlarını önlemek için yardımcı sınıf.
    /// </summary>
    public static class PopupHelper
    {
        /// <summary>
        /// Verilen nesne için düzenlenebilir (Edit Mode) bir DetailView oluşturur.
        /// </summary>
        public static DetailView CreateEditableDetailView(XafApplication application, IObjectSpace objectSpace, object obj)
        {
            DetailView popUpView = application.CreateDetailView(objectSpace, obj);
            popUpView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            return popUpView;
        }

        /// <summary>
        /// Popup'tan dönen sonucu kaydeder ve ana görünümü (View) yeniler.
        /// </summary>
        public static void CommitAndRefresh(PopupWindowShowActionExecuteEventArgs e, View parentView)
        {
            if (e.PopupWindowViewCurrentObject != null)
            {
                IObjectSpace popupObjectSpace = e.PopupWindowView?.ObjectSpace;
                if (popupObjectSpace?.IsModified == true)
                {
                    popupObjectSpace.CommitChanges();
                    
                    // Ana ekranı (Müşteri/Kişi Detayı) yenile
                    parentView?.ObjectSpace?.Refresh();
                }
            }
        }
    }
}
