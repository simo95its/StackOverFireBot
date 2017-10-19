using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using StackExchange.StacMan;

namespace Bot_Application.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        //email: p571585@mvrht.net
        //username: oioidviod
        //password: biribi
        //website: thumbnail.ws
        private const string API_KEY_THUMBNAIL = "abc521d453eb607d9fe60571f9e9adc69ebaee695e16";

        private const string HACKING_TIME_GIF = "https://33.media.tumblr.com/e66f25408a125d0f76bf71c1b82b89b7/tumblr_mz1jujdFyV1qfv72no1_500.gif";

        private const string COMMODORE_SOLUTION = "https://i.makeagif.com/media/6-01-2015/BzxHr5.gif";

        private const string ERROR_GIF = "http://k30.kn3.net/taringa/0/4/A/B/E/6/vagonettas/282.gif";

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            string search = activity.Text;
            var client = new StacManClient(key: "", version: "2.1");

            if (search.Split(' ')[0] == "Yes")
            {
                search = search.Split(' ')[2];
            }

            int page = int.TryParse(search, out page) ? page : 1;

            var carouselReply = context.MakeMessage();
            carouselReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            var cardReply = context.MakeMessage();

            if (search != "No")
            {
                var response = await client.Search.GetMatches("stackoverflow",
                page: page,
                pagesize: 10,
                order: Order.Desc,
                sort: StackExchange.StacMan.Questions.SearchSort.Votes,
                intitle: search);

                if (response.Data.Items.GetLength(0) > 0)
                {
                    foreach (var question in response.Data.Items)
                    {
                        string url = "https://api.thumbnail.ws/api/" + API_KEY_THUMBNAIL + "/thumbnail/get" + "?url=" +
                            question.Link + "&width=" + 640;

                        carouselReply.Attachments.Add(GetHeroCard(question.Title,
                            "Score" + question.Score,
                            "Owner" + question.Owner,
                            new CardImage(url: url),
                            new CardAction(ActionTypes.OpenUrl, "Read more", value: question.Link)));
                    }

                    cardReply.Attachments.Add(GetAnimationCard("More questions?",
                        "Do you want more result?",
                        HACKING_TIME_GIF,
                        new CardAction(ActionTypes.ImBack, "Yes", value: "Yes " + "page " + (++page).ToString()),
                        new CardAction(ActionTypes.ImBack, "No", value: "No")));

                    // return our reply to the user
                    await context.PostAsync(carouselReply);
                    await context.PostAsync(cardReply);
                }
                else
                {
                    cardReply.Attachments.Add(GetAnimationCard("ERROR!",
                        "There aren't result.",
                        ERROR_GIF));

                    // return our reply to the user
                    await context.PostAsync(cardReply);
                }
            }
            else
            {
                cardReply.Attachments.Add(GetAnimationCard("Perfect!",
                    "You've found the solution!",
                    COMMODORE_SOLUTION));

                // return our reply to the user
                await context.PostAsync(cardReply);
            }

            context.Wait(MessageReceivedAsync);
        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction }
            };
            return heroCard.ToAttachment();
        }

        private static Attachment GetAnimationCard(string title, string subtitle, string url, params CardAction[] cardsAction)
        {
            var animationCard = new AnimationCard
            {
                Title = title,
                Subtitle = subtitle,
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = url
                    }
                },
                Buttons = new List<CardAction>()
            };
            foreach (var item in cardsAction)
            {
                animationCard.Buttons.Add(item);
            }
            return animationCard.ToAttachment();
        }
    }
}