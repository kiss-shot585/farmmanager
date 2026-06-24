using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace farmmanager.Helpers
{
    public static class CommandExtensions
    {
        public static async Task ExecuteAsync(this ICommand command, object? parameter = null)
        {
            command.Execute(parameter);
            await Task.Delay(150); // yield so async void handlers can complete
        }
    }
}
