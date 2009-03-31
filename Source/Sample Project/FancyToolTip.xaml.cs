using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sample_Project
{
	/// <summary>
	/// Interaction logic for FancyToolTip.xaml
	/// </summary>
	public partial class FancyToolTip
	{
	  #region InfoText dependency property

	  /// <summary>
	  /// The tooltip details.
	  /// </summary>
	  public static readonly DependencyProperty InfoTextProperty =
	      DependencyProperty.Register("InfoText",
	                                  typeof (string),
	                                  typeof (FancyToolTip),
	                                  new FrameworkPropertyMetadata("", InfoTextPropertyChanged));

	  /// <summary>
	  /// A property wrapper for the <see cref="InfoTextProperty"/>
	  /// dependency property:<br/>
	  /// The tooltip details.
	  /// </summary>
	  public string InfoText
	  {
	    get { return (string) GetValue(InfoTextProperty); }
	    set { SetValue(InfoTextProperty, value); }
	  }


	  /// <summary>
	  /// A static callback listener which is being invoked if the
	  /// <see cref="InfoTextProperty"/> dependency property has
	  /// been changed. Invokes the <see cref="OnInfoTextPropertyChanged"/>
	  /// instance method of the changed instance.
	  /// </summary>
	  /// <param name="d">The currently processed owner of the property.</param>
	  /// <param name="e">Provides information about the updated property.</param>
	  private static void InfoTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	  {
	    FancyToolTip owner = (FancyToolTip) d;
	    owner.OnInfoTextPropertyChanged(e);
	  }


	  /// <summary>
	  /// Handles changes of the <see cref="InfoTextProperty"/> dependency property. As
	  /// WPF internally uses the dependency property system and bypasses the
	  /// <see cref="InfoText"/> property wrapper, updates of the property's value
	  /// should be handled here.
	  /// </summary
	  /// <param name="e">Provides information about the updated property.</param>
	  private void OnInfoTextPropertyChanged(DependencyPropertyChangedEventArgs e)
	  {
//	    string newValue = (string) e.NewValue;
	  }

	  #endregion



		public FancyToolTip()
		{
			this.InitializeComponent();
		}
	}
}