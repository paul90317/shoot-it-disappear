using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;

namespace game
{
    public partial class Form2 : Form
    {
        string server = "https://shoot-it-disappear.herokuapp.com/";
        public Form2()
        {
            InitializeComponent();
        }
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        UdpClient udpClient = new UdpClient();
        public static string name;
        struct Bullet
        {
            public int x, y;
            public Label label;
        };
        struct Player
        {
            public int x, y;
            public Label label;
            public List<Bullet> bullets;
            public int cnt;
            public bool remove;
        };
        Dictionary<string, Player> players = new Dictionary<string, Player>();
        void udp()
        {
            try
            {
                
#pragma warning disable CS0618 // 類型或成員已經過時
                IPAddress ipAddress = Dns.Resolve("shoot-it-disappear.herokuapp.com").AddressList[0];
#pragma warning restore CS0618 // 類型或成員已經過時
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 2222);
                udpClient.Connect(ipEndPoint);
                backgroundWorker1.RunWorkerAsync();

                // Sends a message to the host to which you have connected.
                Byte[] sendBytes = Encoding.ASCII.GetBytes(name);

                udpClient.Send(sendBytes, sendBytes.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void close()
        {
            flag = false;
            timer1.Stop();
            udpClient.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            udp();
            timer1.Interval = 15;
            timer1.Start();
        }

        bool flag = true;
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (flag)
            {
                try
                {
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    string[] cmds = returnData.Split(' ');
                    for (int i = 0; i < cmds.Length; i++)
                    {
                        string tmp = cmds[i];
                        switch (tmp)
                        {
                            case "fail":
                                MessageBox.Show("The name has been registered.");
                                break;
                            case "remove":
                                i++;
                                {
                                    Player player = players[cmds[i]];
                                    player.remove = true;
                                    players[cmds[i]] = player;
                                }
                                break;
                            case "":
                                break;
                            default:
                                if (players.ContainsKey(tmp))
                                {
                                    Player player = players[tmp];
                                    player.x = int.Parse(cmds[i + 1]);
                                    player.y = int.Parse(cmds[i + 2]);
                                    player.cnt = int.Parse(cmds[i + 3]);
                                    i += 4;
                                    for (int j = 0; j < player.cnt; j++)
                                    {
                                        if (j >= player.bullets.Count)
                                        {
                                            player.bullets.Add(new Bullet
                                            {
                                                x = int.Parse(cmds[i + 2 * j]),
                                                y = int.Parse(cmds[i + 2 * j + 1]),
                                                label = new Label
                                                {
                                                    Width = 16,
                                                    Height = 16,
                                                    BackColor = (name == tmp) ? Color.Black : Color.Red,
                                                    Enabled = false
                                                },
                                            });
                                        }
                                        else
                                        {
                                            Bullet b = player.bullets[j];
                                            b.x = int.Parse(cmds[i + 2 * j]);
                                            b.y = int.Parse(cmds[i + 2 * j + 1]);
                                            player.bullets[j] = b;
                                        }
                                    }
                                    i += 2 * player.cnt - 1;
                                    players[tmp] = player;
                                }
                                else
                                {
                                    players[tmp] = new Player {
                                        x = int.Parse(cmds[i + 1]),
                                        y = int.Parse(cmds[i + 2]),
                                        label = new Label
                                        {
                                            Width = 40,
                                            Height = 40,
                                            Text = tmp,
                                            ForeColor = (name == tmp) ? Color.White : Color.Black,
                                            BackColor = (name == tmp) ? Color.Blue : Color.SandyBrown,
                                            //Enabled = false,
                                        },
                                        bullets = new List<Bullet>(),
                                        cnt = int.Parse(cmds[i + 3]),
                                        remove = false
                                    };
                                    Player player = players[tmp];
                                    
                                    i += 4;
                                    for (int j = 0; j < player.cnt; j++)
                                    {
                                        player.bullets.Add(new Bullet
                                        {
                                            x = int.Parse(cmds[i + 2 * j]),
                                            y = int.Parse(cmds[i + 2 * j + 1]),
                                            label = new Label
                                            {
                                                Width = 16,
                                                Height = 16,
                                                BackColor = (name == tmp) ? Color.Black : Color.Red,
                                                Enabled = false
                                            },
                                        });
                                    }
                                    players[tmp] = player;
                                    i += 2 * player.cnt - 1;
                                }
                                
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        void remove(Player p)
        {
            if (Controls.Contains(p.label))
            {
                Controls.Remove(p.label);
                for(int i = 0; i < p.cnt; i++)
                {
                    Controls.Remove(p.bullets[i].label);
                }
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                foreach (var player in players)
                {
                    if (player.Value.remove)
                    {
                        remove(player.Value);
                        continue;
                    }
                    player.Value.label.Location = new Point(player.Value.x, player.Value.y);
                    if (!Controls.Contains(player.Value.label))
                    {
                        Controls.Add(player.Value.label);
                    }
                    for(int i = 0; i < player.Value.cnt; i++)
                    {
                        player.Value.bullets[i].label.Location = new Point(player.Value.bullets[i].x, player.Value.bullets[i].y);
                        if (!Controls.Contains(player.Value.bullets[i].label))
                        {
                            Controls.Add(player.Value.bullets[i].label);
                        }
                    }
                    for(int i = player.Value.cnt; i < player.Value.bullets.Count; i++)
                    {
                        Controls.Remove(player.Value.bullets[i].label);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        static readonly HttpClient client = new HttpClient();
        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            string url = server;
            switch (e.KeyCode)
            {
                case Keys.W:
                    url += "top";
                    break;
                case Keys.S:
                    url += "bottom";
                    break;
                case Keys.A:
                    url += "left";
                    break;
                case Keys.D:
                    url += "right";
                    break;
            }
            url += "?name=" + name;
            Task<HttpResponseMessage> response = client.GetAsync(url);
        }

        private void Form2_KeyUp(object sender, KeyEventArgs e)
        {
            string url = server;
            switch (e.KeyCode)
            {
                case Keys.W:
                    url += "top";
                    break;
                case Keys.S:
                    url += "bottom";
                    break;
                case Keys.A:
                    url += "left";
                    break;
                case Keys.D:
                    url += "right";
                    break;
            }
            url += "Up?name=" + name;
            Task<HttpResponseMessage> response = client.GetAsync(url);
        }

        private void Form2_MouseClick(object sender, MouseEventArgs e)
        {
            string url = server;
            url += "click?name=" + name + "&x=" + e.X + "&y=" + e.Y;
            Task<HttpResponseMessage> response = client.GetAsync(url);
        }
    }
}
