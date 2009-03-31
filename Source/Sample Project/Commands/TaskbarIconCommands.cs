using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Sample_Project.Commands
{
  public static class TaskbarIconCommands
  {
    public static HideMainWindowCommand HideMain { get; set; }
    public static ShowMainWindowCommand ShowMain { get; set; }


    static TaskbarIconCommands()
    {
      HideMain = new HideMainWindowCommand();
      ShowMain  =new ShowMainWindowCommand();
    }

    public static void RefreshCommands()
    {
      HideMain.RaiseCanExcecuteChanged();
      ShowMain.RaiseCanExcecuteChanged();
    }
  }
}
