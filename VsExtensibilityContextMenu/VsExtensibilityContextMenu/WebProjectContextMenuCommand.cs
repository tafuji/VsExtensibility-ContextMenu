using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace VsExtensibilityContextMenu
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class WebProjectContextMenuCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("be561888-6b16-4eab-be7a-404bb2a05f8c");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebProjectContextMenuCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private WebProjectContextMenuCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += new EventHandler(BeforeQueryStatus);
            commandService.AddCommand(menuItem);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            if (_dte == null)
                return;
            EnvDTE.Solution solution = _dte.Solution;
            EnvDTE.Project project = (EnvDTE.Project)((object[])_dte.ActiveSolutionProjects)[0];

            var cmd = (OleMenuCommand)sender;
            cmd.Visible = project.Kind == "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static WebProjectContextMenuCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// DTE object
        /// </summary>
        private EnvDTE.DTE _dte;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Verify the current thread is the UI thread - the call to AddCommand in WebProjectContextMenuCommand's constructor requires
            // the UI thread.
            ThreadHelper.ThrowIfNotOnUIThread();

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new WebProjectContextMenuCommand(package, commandService);
            Instance._dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_dte == null)
                return;
            EnvDTE.Solution solution = _dte.Solution;
            EnvDTE.Project project = (EnvDTE.Project)((object[])_dte.ActiveSolutionProjects)[0];
            string message = $"Selected project is {project.Name}";
            string title = "Web Project Context Menu Command";

            // Show a message
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
