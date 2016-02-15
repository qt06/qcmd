using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using qthk;
namespace qcmd
{
    // 1.定义委托
    public delegate void DelReadStdOutput(string result);
    public delegate void DelReadErrOutput(string result);

    public partial class MainWindow : Form
    {
        // 2.定义委托事件
        public event DelReadStdOutput ReadStdOutput;
        public event DelReadErrOutput ReadErrOutput;
        public Process CmdProcess = null;
        public bool isrun = false;
        public Suggestion suggestion = null;
        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            //3.将相应函数注册到委托事件中
            ReadStdOutput += new DelReadStdOutput(ReadStdOutputAction);
            ReadErrOutput += new DelReadErrOutput(ReadErrOutputAction);
            CmdProcess = new Process();
            CmdProcess.StartInfo.CreateNoWindow = true;         // 不创建新窗口
            CmdProcess.StartInfo.UseShellExecute = false;
            CmdProcess.StartInfo.RedirectStandardInput = true;  // 重定向输入
            CmdProcess.StartInfo.RedirectStandardOutput = true; // 重定向标准输出
            CmdProcess.StartInfo.RedirectStandardError = true;  // 重定向错误输出
            //CmdProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            CmdProcess.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
            CmdProcess.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);

            CmdProcess.EnableRaisingEvents = true;                      // 启用Exited事件
            CmdProcess.Exited += new EventHandler(CmdProcess_Exited);   // 注册进程结束事件
            this.suggestion = new Suggestion(Application.StartupPath + "\\qcmd.ini", "qcmd");
        }

        private void RealAction(string StartFileName, string StartFileArg)
        {
            if (!this.isrun)
            {
                CmdProcess.StartInfo.FileName = StartFileName;      // 命令
                CmdProcess.StartInfo.Arguments = "";      // 参数
                CmdProcess.Start();
                CmdProcess.BeginOutputReadLine();
                CmdProcess.BeginErrorReadLine();
            }
            CmdProcess.StandardInput.WriteLine(StartFileArg);
            this.isrun = true;

            // 如果打开注释，则以同步方式执行命令，此例子中用Exited事件异步执行。
            //CmdProcess.WaitForExit();     
        }

        private void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                // 4. 异步调用，需要invoke
                this.Invoke(ReadStdOutput, new object[] { e.Data });
            }
        }

        private void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                this.Invoke(ReadErrOutput, new object[] { e.Data });
            }
        }

        private void ReadStdOutputAction(string result)
        {
            this.resultRichTextBox.AppendText(result + "\r\n");
            snd.play("out.wav");
        }

        private void ReadErrOutputAction(string result)
        {
            snd.play("out.wav");
            this.resultRichTextBox.AppendText(result + "\r\n");
        }

        private void CmdProcess_Exited(object sender, EventArgs e)
        {
            // 执行结束后触发
            CmdProcess.CancelErrorRead();
            CmdProcess.CancelOutputRead();
            this.isrun = false;
            snd.play("exit.wav");
        }

        [DllImport("kernel32.dll")]
        public static extern bool GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCtrlHandler(IntPtr handlerRoutine, bool add);

        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();

        private void button1_Click(object sender, EventArgs e)
        {
            string cmd = this.cmdTextBox.Text.Trim();
            if (string.IsNullOrEmpty(cmd))
            {
                this.cmdTextBox.Focus();
                return;
            }
            this.suggestion.Add(cmd);
            this.cmdTextBox.Items.AddRange(this.suggestion.SuggestionList.ToArray());
            //如果输入命令为cls则清空结果框
            if (cmd.ToLower() == "cls")
            {
                this.resultRichTextBox.Text = "";
            }
            //如果输入命令为exit则退出
            if (cmd.ToLower() == "exit")
            {
                Application.Exit();
            }
            // 启动进程执行相应命令,此例中以执行ping.exe为例
            RealAction("cmd.exe", cmdTextBox.Text.Trim());
            snd.begin();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C)
            {
                if (this.isrun)
                {
                    AttachConsole(CmdProcess.Id); // 附加到目标进程的console
                    SetConsoleCtrlHandler(IntPtr.Zero, true); // 设置自己的ctrl+c处理，防止自己被终止
                    GenerateConsoleCtrlEvent(0, 0); // 发送ctrl+c（注意：这是向所有共享该console的进程发送）
                    FreeConsole(); // 脱离目标console
                    snd.exit();
                }
                e.SuppressKeyPress = true;
                return;
            }
            if (e.KeyCode == Keys.Enter)
            {
                this.button1.PerformClick();
                e.SuppressKeyPress = true;
                return;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.cmdTextBox.Items.AddRange(this.suggestion.SuggestionList.ToArray());
            this.cmdTextBox.Text = "在这里输入命令，按回车执行：";
            this.cmdTextBox.SelectAll();
        }

        //end class
    }
}