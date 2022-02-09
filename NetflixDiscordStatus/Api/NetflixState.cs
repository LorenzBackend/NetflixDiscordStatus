using DiscordRPC;
using NetflixDiscordStatus.Misc;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetflixDiscordStatus.Api
{
    //NetflixDiscordStatus by LrnzCode
    //https://github.com/lrnzcode/NetflixDiscordStatus
    public class NetflixState
    {
        private static DiscordRpcClient client;
        private static string[] states = { "Browse the Netflix world" };
        private static RpcState currentState;
        private static string currentMovie = "";
        private enum RpcState
        {
            Browse,
            Move,
            Series
        }
        public static void Init()
        {
            try
            {
                client = new DiscordRpcClient("940447627282104330");
                client.Initialize();
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
            currentState = RpcState.Browse;
            currentMovie = "";
            client.SetPresence(new RichPresence()
            {
                State = "",
                Details = states[0],
                Timestamps = Timestamps.Now,
                Assets = new Assets()
                {
                    LargeImageKey = "app_icon"
                },
                Buttons = new Button[]
                {
                    new Button() { Label = "GET THE APP", Url = "https://github.com/lrnzcode/NetflixDiscordStatus/releases" }
                }       
            });
        }

        private static void CheckForNewState()
        {
            while (true)
            {
                Thread.Sleep(250);

                IWebElement sTitle = Core.FindElementWhenExists(By.CssSelector("#appMountPoint > div > div > div.watch-video > div > div > div.ltr-1420x7p > div.watch-video--bottom-controls-container.ltr-hpbgml > div > div > div.ltr-1bt0omd > div > div.ltr-1fkysoc > div.medium.ltr-qnht66 > h4"));
                IWebElement subTitle = Core.FindElementWhenExists(By.CssSelector("div > div.watch-video--bottom-controls-container.ltr-hpbgml > div > div > div.ltr-1bt0omd > div > div.ltr-1fkysoc > div.medium.ltr-qnht66 > span:nth-child(2)"));
                IWebElement movieTitle = Core.FindElementWhenExists(By.CssSelector("#appMountPoint > div > div > div.watch-video > div > div > div.ltr-1420x7p > div.watch-video--bottom-controls-container.ltr-hpbgml > div > div > div.ltr-1bt0omd > div > div.ltr-1fkysoc > div.medium.ltr-qnht66"));

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
                            currentState = RpcState.Move;
                            UpdateState("Watching", movieTitleText);
                        }
                        catch
                        {
                            //hm i dont know... do nothing
                        }
             
                    }
                }
            }
        }

        public static void UpdateState(string detail, string state)
        {
            if (currentState == RpcState.Move && state == currentMovie) return;
            if (currentState == RpcState.Series && state == currentMovie) return;

            if (currentState == RpcState.Move) currentMovie = detail;
            if (currentState == RpcState.Series) currentMovie = state;

            client.UpdateState(state);
            client.UpdateDetails(detail);

            client.SetPresence(new RichPresence()
            {
                State = state,
                Details = detail,
                Timestamps = Timestamps.Now,
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
