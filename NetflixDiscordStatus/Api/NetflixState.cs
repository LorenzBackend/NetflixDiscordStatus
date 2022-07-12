using DiscordRPC;
using NetflixDiscordStatus.Misc;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetflixDiscordStatus.Api
{
    //NetflixDiscordStatus by LorenzBackend
    //https://github.com/LorenzBackend/NetflixDiscordStatus
    public class NetflixState
    {
        private static DiscordRpcClient client;

        private static string[] states = { "Browse the Netflix world" };
        private static RpcState currentState;
        private static string currentMovie = "";
        private enum RpcState
        {
            Browse,
            Movie,
            Series,
            None
        }
        public static void Init()
        {

            try
            {
                currentState = RpcState.None;
                client = new DiscordRpcClient("DiscordClient");
                client.Initialize();
                SetBrowseState();

                Thread th = new Thread(CheckForNewState);
                th.Start();
            }
            catch (Exception ex)
            {
                MyEventHandler.SendUnexpectedError(ex.Message);
            }
        }

        private static void SetBrowseState()
        {
            currentMovie = "";
            Timestamps time;
            if (client.CurrentPresence != null)
            {
                time = client.CurrentPresence.Timestamps;
            }
            else
            {
                time = Timestamps.Now;
            }

            client.SetPresence(new RichPresence()
            {
                State = "",
                Details = states[0],
                Timestamps = time,
                Assets = new Assets()
                {
                    LargeImageKey = "app_icon"
                },
                Buttons = new Button[]
                {
                    new Button() { Label = "GET THE APP", Url = "https://github.com/lrnzcode/NetflixDiscordStatus/releases" }
                }       
            });

            currentState = RpcState.Browse;
        }

        private static void CheckForNewState()
        {
            while (true)
            {
                Thread.Sleep(250);

                IWebElement sTitle = Core.FindElementWhenExists(By.CssSelector("h4:nth-child(1)"));
                IWebElement subTitle = Core.FindElementWhenExists(By.CssSelector(".medium > span:nth-child(2)"));
                IWebElement movieTitle = Core.FindElementWhenExists(By.CssSelector(".ltr-weehum-videoTitleCss"));

                if (sTitle == null && subTitle == null && movieTitle == null)
                {
                    if (Core.GetUrl().Contains("/browse") && currentState != RpcState.Browse) SetBrowseState();
                }

                if (sTitle != null && subTitle != null)
                {
                    try
                    {
                        string titleText = sTitle.Text;
                        string subTitleText = subTitle.Text;

                        if (titleText.Length > 32) titleText = titleText.Substring(0, 32);
                        if (subTitleText.Length > 32) subTitleText = subTitleText.Substring(0, 32);

                        currentState = RpcState.Series;
                        UpdateState(titleText, subTitleText);
                    }
                    catch
                    {
                        //hm i dont know... do nothing
                    }

                }
                else
                {
                    if (movieTitle != null)
                    {
                        try
                        {
                            string movieTitleText = movieTitle.Text;
                            if (movieTitleText.Length > 32) movieTitleText = movieTitleText.Substring(0, 32);
                            currentState = RpcState.Movie;
                            UpdateState("Watching", movieTitleText);
                        }
                        catch
                        {
                            //hm i dont know... do nothing
                        }  
                    }
                }



                if (currentState == RpcState.Movie || currentState == RpcState.Series)
                {
                    IWebElement playerState = Core.FindElementWhenExists(By.CssSelector(".svg-icon-nfplayerPause > path"));

                    if (playerState != null)
                    {

                    }
                }
            }
        }

 
        public static void UpdateState(string detail, string state)
        {
            if (currentState == RpcState.Movie && state == currentMovie || currentState == RpcState.Series && state == currentMovie) return;

            if (currentState == RpcState.Movie) currentMovie = detail;
            if (currentState == RpcState.Series) currentMovie = state;

            client.UpdateState(state);
            client.UpdateDetails(detail);

            Timestamps time;
            if (client.CurrentPresence != null)
            {
                time = client.CurrentPresence.Timestamps;
            }
            else
            {
                time = Timestamps.Now;
            }

            client.SetPresence(new RichPresence()
            {
                State = state,
                Details = detail,
                Timestamps = time,
                Assets = new Assets()
                {
                    LargeImageKey = "app_icon"
                },
                Buttons = new Button[]
                {
                    new Button() { Label = "Watch " + GetButtonText(detail, state), Url = GetMovieUrl()},
                    new Button() { Label = "GET THE APP", Url = main.ReleasesUrl}
                }

            });
        }

        private static string GetButtonText(string detail, string state)
        {
            if (currentState == RpcState.Series)
            {
                return detail;
            }
            else
            {
                return state;
            }
        }
        private static string GetMovieUrl()
        {
            string url = Core.GetUrl();
            int index = url.IndexOf("?");
            return url.Substring(0, index);
        }

        public static void Shutdown()
        {
            if (client != null)
            {
                client.Dispose();
            }
        }
    }
}
