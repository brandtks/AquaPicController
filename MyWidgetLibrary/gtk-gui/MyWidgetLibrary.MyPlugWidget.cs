
// This file has been generated by the GUI designer. Do not modify.
namespace MyWidgetLibrary
{
	public partial class MyPlugWidget
	{
		private global::Gtk.DrawingArea area;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MyWidgetLibrary.MyPlugWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "MyWidgetLibrary.MyPlugWidget";
			// Container child MyWidgetLibrary.MyPlugWidget.Gtk.Container+ContainerChild
			this.area = new global::Gtk.DrawingArea ();
			this.area.WidthRequest = 90;
			this.area.HeightRequest = 90;
			this.area.Events = ((global::Gdk.EventMask)(768));
			this.area.Name = "area";
			this.Add (this.area);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
			this.area.ExposeEvent += new global::Gtk.ExposeEventHandler (this.OnAreaExposeEvent);
			this.area.ButtonPressEvent += new global::Gtk.ButtonPressEventHandler (this.OnAreaButtonClickedEvent);
		}
	}
}
