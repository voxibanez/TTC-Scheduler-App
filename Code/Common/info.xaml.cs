using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace hello
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class info : ContentPage
	{
		public info ()
		{
			InitializeComponent ();
		}
	}
}