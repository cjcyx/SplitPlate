using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SplitPlate
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class SplitPlateClass : Window
    {
        public delegate void eventDelegate(ArrayList args);
        public eventDelegate eventInvok;
        private static string serverPath = "http://172.16.5.130:8082/";
        DateTime date;
        public SplitPlateClass()
        {
            date = DateTime.Now;
            //TimeSpan timeSpan;
            //string message;
            //if (! Activation.GetActivationInfo.Get(out timeSpan, out message))
            //{
            //    MessageBox.Show(message);
            //    var From = new Activation.InputAuth();
            //    From.ShowDialog();
            //    Environment.Exit(0);
            //    this.Close();
            //}
            //else if (timeSpan.TotalDays < 3)
            //{
            //    MessageBox.Show("离线许可剩余时间为" + ((int)timeSpan.TotalDays).ToString() + "天！");
            //    InitializeComponent();
            //}
            //else
            {
                InitializeComponent();
            }
            this.Closing += SplitPlateClass_Closing;
            this.Loaded += Window_Loaded;
        }
        public void OnTop()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            TopMostTool.SetTopmost(hwnd);
            WindowState = WindowState.Normal;
        }
        public void OnBot()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            TopMostTool.UnSetTopmost(hwnd);
            WindowState = WindowState.Minimized;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OnTop();
        }
        private void SplitPlateClass_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DateTime date1 = DateTime.Now;
            var task = Task.Factory.StartNew(() =>
            {
                PostLog(date1);
            });
            ArrayList list = new ArrayList();
            list.Add("!!SplitObject.ClearAID()");
            list.Add("!!SplitObject.ClearAIDCirc()");
            list.Add("!!SplitObject.OnClose()");
            eventInvok(list);
        }
        private void Run_Click(object sender, RoutedEventArgs e)
        {
            ArrayList list = new ArrayList();
            list.Add("!!SplitObject.GetPoints('" + PaneRef.Text + "')");
            eventInvok(list);
        }
        private void numKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!((e.Key >= Key.D0 && e.Key <= Key.D9)             //大键盘0-9
                || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)   //小键盘0-9 
                || e.Key == Key.Delete || e.Key == Key.Enter
                || e.Key == Key.Back || e.Key == Key.Decimal
                || e.Key == Key.Right || e.Key == Key.Left || e.Key == Key.Tab || e.Key == Key.OemMinus || e.Key == Key.Subtract))
            {
                e.Handled = true;//不可输入
            }
        }
        private void TextChanged(object sender, TextChangedEventArgs ea)
        {
            ReAddLine();
        }
        public void ReAddLine()
        {
            ArrayList list = new ArrayList();
            try
            {
                list.Add("!!SplitObject.ClearAID()");
                double angle = Convert.ToDouble(ang.Text);
                double angl = (angle % 180) * Math.PI / 180;
                double e = Convert.ToDouble(ePOS.Text) + (double)ePOS.Tag;
                double n = Convert.ToDouble(nPOS.Text) + (double)nPOS.Tag;
                double distLen = Convert.ToDouble(dist.Text);
                if (splitOnly.IsChecked == true && Convert.ToDouble(yLen.Text) > 0)
                {
                    double splitDist = Convert.ToDouble(xLen.Text);
                    double splitDistAbs = Math.Abs(splitDist);
                    double delte = (distLen / 2 + splitDistAbs) * Math.Sin(angl);
                    double deltn = -(distLen / 2 + splitDistAbs) * Math.Cos(angl);
                    if (splitDist > 0)
                    {
                        e += delte;
                        n += deltn;
                    }
                    else
                    {
                        e -= delte;
                        n -= deltn;
                    }
                    for (int i = 0; i < Convert.ToDouble(yLen.Text); i++)
                    {
                        list.Add("!!SplitObject.AddAID('E " + e.ToString() + " N " + n.ToString() + " U0 wrt " + PaneRef.Text + "'," + angle.ToString() + ")");
                         delte = (distLen + splitDistAbs) * Math.Sin(angl);
                        deltn = -(distLen + splitDistAbs) * Math.Cos(angl);
                        if (splitDist > 0)
                        {
                            e += delte;
                            n += deltn;
                        }
                        else
                        {
                            e -= delte;
                            n -= deltn;
                        }
                    }
                }
                else
                {
                    list.Add("!!SplitObject.AddAID('E " + e.ToString() + " N " + n.ToString() + " U0 wrt " + PaneRef.Text + "'," + angle.ToString() + ")");
                }
            }
            catch
            {
                return;
            }
            eventInvok(list);
            AidCircle();
        }
        private void GetPANE_Click(object sender, RoutedEventArgs e)
        {
            ArrayList list = new ArrayList();
            list.Add("!!SplitObject.GetPANE()");
            eventInvok(list);
        }
        public void RunCommand(ArrayList list)
        {
            if (AutoDel.IsChecked == true)
            {
                list.Add("!!SplitObject.DeletePANE('" + PaneRef.Text + "')");
                PaneRef.Text = "";
            }
            list.Add("!!SplitObject.ClearAID()");
            list.Add("!!SplitObject.ClearAIDCirc()");
            eventInvok(list);
        }
        private void GetPIPE_Click(object sender, RoutedEventArgs e)
        {
            if (PaneRef.Text != "")
            {
                OnBot();
                ArrayList list = new ArrayList();
                if (splitOnly.IsChecked != true)
                {
                    list.Add("!!SplitObject.GetPostion('" + PaneRef.Text + "')");
                }
                else
                {
                    list.Add("!!SplitObject.GetPostionSplitOnly('" + PaneRef.Text + "')");
                }
                eventInvok(list);
            }
        }
        private void Checked(object sender, RoutedEventArgs e)
        {
            if (yuan.IsChecked == true)
            {
                xF.Text = "管道外径";
                yF.Text = "";
                xLen.Visibility = Visibility.Visible;
                yLen.Visibility = Visibility.Hidden;
                xLen.IsEnabled = true;
                yLen.IsEnabled = false;
                Change.Visibility = Visibility.Collapsed;
                AidCircle();
            }
            else if (fang.IsChecked == true)
            {
                xF.Text = "X长度";
                yF.Text = "Y长度";
                xLen.Visibility = Visibility.Visible;
                yLen.Visibility = Visibility.Visible;
                xLen.IsEnabled = true;
                yLen.IsEnabled = true;
                Change.Visibility = Visibility.Visible;
                AidCircle();
            }
            else
            {
                xF.Text = "板宽";
                yF.Text = "分割次数";
                xLen.Visibility = Visibility.Visible;
                yLen.Visibility = Visibility.Visible;
                xLen.IsEnabled = true;
                yLen.IsEnabled = true;
                yLen.Text = "1";
                xLen.Text = "995";
                Change.Visibility = Visibility.Collapsed;
                RemoveAidCircle();
            }
        }
        private void RemoveAidCircle()
        {
            ArrayList list = new ArrayList();
            list.Add("!!SplitObject.ClearAIDCirc()");
            eventInvok(list);
        }
        private void AidCircle()
        {
            try
            {
                if (yuan.IsChecked == true)
                {
                    ArrayList list = new ArrayList();
                    double e = Convert.ToDouble(ePOS.Text) + (double)ePOS.Tag;
                    double n = Convert.ToDouble(nPOS.Text) + (double)nPOS.Tag;
                    double diam = Convert.ToDouble(xLen.Text);
                    double diam1 = Convert.ToDouble(xLen.Text) + 2 * Convert.ToDouble(dist1.Text);
                    list.Add("!!SplitObject.ClearAIDCirc()");
                    list.Add("!!SplitObject.AddCircel('E " + e.ToString() + " N " + n.ToString() + " U0 wrt " + PaneRef.Text + "'," + diam.ToString() + ")");
                    list.Add("!!SplitObject.AddCircel('E " + e.ToString() + " N " + n.ToString() + " U0 wrt " + PaneRef.Text + "'," + diam1.ToString() + ")");
                    eventInvok(list);
                }
                else if (fang.IsChecked == true)
                {
                    ArrayList list = new ArrayList();
                    double e = Convert.ToDouble(ePOS.Text) + (double)ePOS.Tag;
                    double n = Convert.ToDouble(nPOS.Text) + (double)nPOS.Tag;
                    double x = Convert.ToDouble(xLen.Text);
                    double x1 = Convert.ToDouble(xLen.Text) + 2 * Convert.ToDouble(dist1.Text);
                    double y = Convert.ToDouble(yLen.Text);
                    double y1 = Convert.ToDouble(yLen.Text) + 2 * Convert.ToDouble(dist1.Text);
                    double angle = Convert.ToDouble(ang.Text);
                    list.Add("!!SplitObject.ClearAIDCirc()");
                    list.Add("!!SplitObject.AddSquare('E " + e.ToString() + " N " + n.ToString() + " U0 wrt " + PaneRef.Text + "'," + x.ToString() + ", " + y.ToString() + "," + angle.ToString() + " )");
                    list.Add("!!SplitObject.AddSquare('E " + e.ToString() + " N " + n.ToString() + " U0 wrt " + PaneRef.Text + "'," + x1.ToString() + ", " + y1.ToString() + ", " + angle.ToString() + ")");
                    eventInvok(list);
                }
            }
            catch
            {
                return;
            }
        }

        private void xLen_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (yuan.IsChecked == true)
            //{
            //    AidCircle();
            //}
            //else if (splitOnly.IsChecked == true)
            {
                ReAddLine();
            }
        }

        private void dist1_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (yuan.IsChecked == true)
            //{
            //    AidCircle();
            //}
            //else if (splitOnly.IsChecked == true)
            {
                ReAddLine();
            }
        }
        private string PostLog(DateTime date1)
        {
            string UserName = Environment.UserName;
            string CompName = Environment.MachineName;
            string MAC = Environment.OSVersion.ToString();
            var data = CompName + "&$&" + UserName + "&$&" + MAC + "&$&" + date.ToString() + "&$&" + date1.ToString();
            data = "data=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
            var result = PostWebRequest(serverPath + "E3DAddINUsage.php", data, Encoding.UTF8);
            return result;
        }
        private bool OnRemoteCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //if (certificate.GetSerialNumberString() == "0337D81016B13293A25301062D7013999224")
            //{
            //    return true;  // 认证正常，没有错误
            //}
            //else
            //{
            //    return false;
            //}
            return true;
        }
        private string PostWebRequest(string postUrl, string paramData, Encoding dataEncode)
        {
            string ret = string.Empty;
            try
            {
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(OnRemoteCertificateValidationCallback);
                byte[] byteArray = dataEncode.GetBytes(paramData); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";
                webReq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36";

                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
            }
            catch (Exception ex)
            {
                ret = ex.Message;
            }
            return ret;
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            var tmp = xLen.Text;
            xLen.Text = yLen.Text;
            yLen.Text = tmp;
        }
    }
    public class TopMostTool
    {
        public static int SW_SHOW = 5;
        public static int SW_NORMAL = 1;
        public static int SW_MAX = 3;
        public static int SW_HIDE = 0;
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);    //窗体置顶
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);    //取消窗体置顶
        public const uint SWP_NOMOVE = 0x0002;    //不调整窗体位置
        public const uint SWP_NOSIZE = 0x0001;    //不调整窗体大小
        public bool isFirst = true;

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        /// <summary>
        /// 查找子窗口
        /// </summary>
        /// <param name="hwndParent"></param>
        /// <param name="hwndChildAfter"></param>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        /// <summary>
        /// 窗体置顶，可以根据需要传入不同的值(需要置顶的窗体的名字Title)
        /// </summary>
        public static void SetTopWindow()
        {
            IntPtr frm = FindWindow(null, "窗体的名字Title");    // 程序中需要置顶的窗体的名字
            if (frm != IntPtr.Zero)
            {
                SetWindowPos(frm, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

                var child = FindWindowEx(frm, IntPtr.Zero, null, "子窗体的名字Title");
            }
        }
        public static void UnSetTopmost(IntPtr handle)
        {
            SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }
        public static void SetTopmost(IntPtr handle)
        {
            SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }
    }
}
