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
            currentState = RpcState.Browse;
            client.SetPresence(new RichPresence()
            {
                State = "",
                Details = states[0],
                Timestamps = Timestamps.Now,
                Assets = new Assets()
                {
                    LargeImageKey = "app_icon"
                },
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
                    string titleText = sTitle.Text;
                    string subTitleText = subTitle.Text;

                    if (titleText.Length > 32) titleText = titleText.Substring(0, 32);
                    if (subTitleText.Length > 32) subTitleText = subTitleText.Substring(0, 32);

                    currentState = RpcState.Series;
                    UpdateState(titleText, subTitleText);
                }
                else
                {
                    if (movieTitle != null)
                    {
                        string movieTitleText = movieTitle.Text;
                        if (movieTitleText.Length > 32) movieTitleText = movieTitleText.Substring(0, 32);
                        currentState = RpcState.Move;
                        UpdateState("Watching", movieTitleText);
                    }
                }
            }
        }

        public static void UpdateState(string detail, string state)
        {
            if (currentState == RpcState.Move && state == currentMovie) return;
            if (currentState == RpcState.Series && detail == currentMovie) return;

            if (currentState == RpcState.Move) currentMovie = state;
            if (currentState == RpcState.Series) currentMovie = detail;

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
                    new Button() { Label = "Watch " + detail, Url = GetMovieUrl() }
                }

            });

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
