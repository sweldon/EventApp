using System;
using Xamarin.Forms;
using EventApp.Models;
using EventApp.ViewModels;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Plugin.Share;
using Plugin.Share.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using Rg.Plugins.Popup.Extensions;
using FFImageLoading.Forms;
using Stormlion.PhotoBrowser;
using System.Collections;
#if __IOS__
using UIKit;
#endif
namespace EventApp.Views
{
    public partial class HolidayDetailPage : ContentPage
    {

        public bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
            set
            {
                if (Settings.IsLoggedIn == value)
                    return;
                Settings.IsLoggedIn = value;
                OnPropertyChanged();
            }
        }

        public bool isPremium
        {
            get { return Settings.IsPremium; }
            set
            {
                if (Settings.IsPremium == value)
                    return;
                Settings.IsPremium = value;
                OnPropertyChanged();
            }
        }

        public string currentUser
        {
            get { return Settings.CurrentUser; }
            set
            {
                if (Settings.CurrentUser == value)
                    return;
                Settings.CurrentUser = value;
                OnPropertyChanged();
            }
        }


        public string devicePushId
        {
            get { return Settings.DevicePushId; }
            set
            {
                if (Settings.DevicePushId == value)
                    return;
                Settings.DevicePushId = value;
                OnPropertyChanged();
            }
        }

        HolidayDetailViewModel viewModel;

        public Comment Comment { get; set; }

        private ObservableCollection<Post> HolidayPosts;
        private bool isAndroidAddPostSubscribed;
        public HolidayDetailPage(HolidayDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = this.viewModel = viewModel;
            HolidayPosts = new ObservableCollection<Post>();
            //NavigationPage.SetHasNavigationBar(this, false);
            // Remove when reply button added
            PostList.ItemSelected += OnCommentSelected;

            // Infinite scrolling for comments
            //PostList.ItemAppearing += (sender, e) =>
            //{

            //    if (viewModel.IsBusy || viewModel.GroupedCommentList.Count == 0)
            //    {
            //        return;
            //    }
            //    //LoadingCommentsDialog.IsVisible = true;
            //    var group = e.Item as CommentList;
            //    if (viewModel.GroupedCommentList.Last() == group)
            //    {
            //        viewModel.GetMoreComments.Execute(null);
            //    }
            //    //LoadingCommentsDialog.IsVisible = false;
            //};


            // TODO: just add comment to sublist dont refresh the whole thing
            //MessagingCenter.Subscribe<HolidayDetailPage, Object[]>(this,
            //"UpdateCelebrateStatus", (sender, data) => {
            //    UpdateCelebrateStatus((string)data[0], (bool)data[1], (string)data[2]);
            //});


        }
        //private void SwipeBack(object sender, SwipedEventArgs e)
        //{
        //    Debug.WriteLine("asdfasdfafds");
        //    Navigation.PopAsync();
        //}
        async Task UpdateHoliday()
        {
            viewModel.Holiday = await viewModel.HolidayStore.GetHolidayById(viewModel.HolidayId);
            // Update celebrate status on detail page
            UpVoteImage.Source = viewModel.Holiday.CelebrateStatus.Contains("active") ? "celebrate_active.png" : "celebrate.png";
            // Update statuses in feed off screen
            bool upvote = viewModel.Holiday.CelebrateStatus.Contains("active") ? true : false;
            Object[] values = { viewModel.Holiday.Name, upvote, viewModel.Holiday.Votes };
            MessagingCenter.Send(this, "UpdateCelebrateStatus", values);
        }

        async void OpenProfile(object sender, EventArgs args)
        {
            dynamic item;
            try
            {
                item = (sender as ContentView).BindingContext as dynamic;
            }
            catch
            {
                item = (sender as ContentView).BindingContext as dynamic;
            }
            Debug.WriteLine($"{item}");
            if (item.Content == "[deleted]" || item.Content == "[blocked]" || item.Content == "[reported]")
                return;

            string UserName = item.UserName;
            await Navigation.PushAsync(new UserPage(user: null, userName: UserName));
        }

        async void DeletePost(Post post=null, Comment comment=null, dynamic container=null)
        {
            string endpoint = null;
            dynamic entity = null;
            if(post != null)
            {
                endpoint = "posts";
                entity = post;
            }
            else if(comment != null)
            {
                endpoint = "comments";
                entity = comment;
            }
            try
            {
                //var post = (sender as Label).BindingContext as Post;
                if (String.Equals(entity.UserName, currentUser,
                        StringComparison.OrdinalIgnoreCase))
                {
                    var deleteComment = await DisplayAlert("Delete Post",
                    "Are you sure you want to delete this post?", "Yes", "No");
                    if (deleteComment)
                    {

                        var values = new Dictionary<string, string>{
                        { "username", currentUser },
                        { "device_id", devicePushId },
                        { "deleted", "1" }
                        };

                        var content = new FormUrlEncodedContent(values);
                        var response = await App.globalClient.PatchAsync(App.HolidailyHost + $"/{endpoint}/{entity.Id}/", content);

                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                        if (response.IsSuccessStatusCode)
                        {
                            entity.UserName = "[deleted]";
                            entity.Content = "[deleted]";
                            entity.ShowReply = "false";
                            entity.ShowDelete = "false";
                            entity.Avatar = "default_user_128.png";
                            entity.ShowEdit = "False";
                            // Disable voting
                            entity.Enabled = false;
                        }
                        else
                        {
                            await DisplayAlert("Error", "Could not edit comment, please try again", "OK");
                        }

                    }

                }
                Utils.RefreshElement(container);
            }
            catch
            {
                await DisplayAlert("Error", "Please try that again", "OK");
            }
            
        }

        async void OnDeleteTapped(object sender, EventArgs args)
        {

            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
            }
            else
            {
                try
                {
                    var item = (sender as Label).BindingContext as Comment;
                    if (String.Equals(item.UserName, currentUser,
                           StringComparison.OrdinalIgnoreCase))
                    {
                        var deleteComment = await DisplayAlert("Delete Comment",
                        "Are you sure you want to delete this comment?", "Yes", "No");
                        if (deleteComment)
                        {

                            var values = new Dictionary<string, string>{
                           { "username", currentUser },
                           { "delete", item.Id },
                           { "device_id", devicePushId }
                            };

                            var content = new FormUrlEncodedContent(values);
                            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/comments/", content);

                            var responseString = await response.Content.ReadAsStringAsync();
                            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                            int status = responseJSON.status;
                            string message = responseJSON.message;
                            if (status == 200)
                            {
                                item.UserName = "[deleted]";
                                item.Content = "[deleted]";
                                item.ShowReply = "False";
                                item.ShowDelete = "False";
                                item.Avatar = "default_user_32.png";
                                item.ShowEdit = "False";
                                // Disable voting
                                item.Enabled = false;
                                item.ElementOpacity = .2;
                                //MessagingCenter.Send(this, "UpdateComments");
                                //await Navigation.PopAsync();
                            }
                            else
                            {
                                await DisplayAlert("Error", message, "OK");
                            }

                        }

                    }
                }
                catch
                {
                    await DisplayAlert("Error", "Please try that again", "OK");
                }
            }
        }

        async void OnEditTapped(object sender, EventArgs args)
        {
            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
            }
            else
            {
                var item = (sender as Label).BindingContext as Comment;
                await Navigation.PushPopupAsync(new NewCommentPopUp(viewModel.Holiday, item, edit: true));
            }
        }


        async void Like(object sender, EventArgs args)
        {
            dynamic entity = (sender as StackLayout).BindingContext;
            string ep = entity.GetType() == typeof(Post) ? "posts" : "comments";
            Utils.Vibrate();

            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
                return;
            }

            if (!entity.LikeEnabled)
                return;
            entity.LikeEnabled = false;
            bool isLiked = entity.LikeImage == "like_neutral.png" ? true : false;


            entity.LikeImage = isLiked == false ? "like_neutral.png" : "like_active.png";
            entity.LikeTextColor = isLiked == false ? Color.FromHex("808080"): Color.FromHex("4c96e8");

            if (isLiked)
            {
                await (sender as StackLayout).ScaleTo(1.5, 50);
                await (sender as StackLayout).ScaleTo(1, 50);
                entity.Likes += 1;
              
                if (entity.Likes > 1)
                    entity.LikeLabel = "Likes";
                else
                    entity.LikeLabel = "Like";
              
            }
            else
            {
                entity.Likes -= 1;
                
                if (entity.Likes > 1)
                    entity.LikeLabel = "Likes";
                else
                    entity.LikeLabel = "Like";
            
            }

            entity.ShowReactions = entity.Likes > 0 ? true : false;
            // Need to update height
            Utils.RefreshElement((sender as StackLayout));

            try
            {
                var values = new Dictionary<string, string>{
                   { "username", currentUser },
                   { "device_id", devicePushId },
                   { "like", isLiked.ToString() },
                };
                var content = new FormUrlEncodedContent(values);
                await App.globalClient.PatchAsync(App.HolidailyHost +
                    $"/{ep}/" + entity.Id + "/", content);


            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Could not like post: {ex}");
            }
            finally
            {
                await Task.Delay(1000);
                entity.LikeEnabled = true;
            }
        }

        async void ReplyPost(object sender, EventArgs args)
        {

            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
            }
            else
            {
                
               Post post = (sender as StackLayout).BindingContext as Post;
                await Navigation.PushPopupAsync(new NewCommentPopUp(
                    viewModel.Holiday, entity: post, reply: true,
                    container: (sender as StackLayout), post: post));
            }
        }

        async void ReplyComment(object sender, EventArgs args)
        {
            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
            }
            else
            {
                
               Comment comment = (sender as StackLayout).BindingContext as Comment;

                //(sender as StackLayout).Parent.Parent.Parent.Parent.Parent.BindingContext
                var post = (sender as StackLayout).Parent;
                while (post.BindingContext.GetType() != typeof(Post))
                {
                    post = post.Parent;
                }

                Post parentPost = post.BindingContext as Post;
                await Navigation.PushPopupAsync(new NewCommentPopUp(
                    viewModel.Holiday, entity: comment, reply: true, container: (sender as StackLayout), post: parentPost));
            }
        }



        async void ReportPost(Post post = null, Comment comment = null, dynamic container=null)
        {
            string endpoint = null;
            dynamic entity = null;
            if (post != null)
            {
                endpoint = "posts";
                entity = post;
            }
            else if (comment != null)
            {
                endpoint = "comments";
                entity = comment;
            }
            var confirm = await DisplayAlert("Report", $"Are you sure " +
                $"you'd like to report this content by {entity.UserName}?", "Yes", "No");
            if (confirm)
            {
                entity.ShowReport = false;
                bool block = false;
                var confirmBlock = await DisplayAlert("Reported", $"Thank you for " +
                    $"making Holidaily a safer place. Would you like to block " +
                    $"future content from {entity.UserName}?", "Yes", "No");
                if (confirmBlock)
                {
                    await DisplayAlert($"{entity.UserName} blocked",
                        $"{entity.UserName} has been blocked", "OK");
                    entity.Content = "[blocked]";
                    entity.UserName = "[blocked]";
                    block = true;
                }
                else
                {
                    entity.Content = "[reported]";
                    entity.UserName = "[reported]";
                }
                entity.Avatar = "default_user_128.png";
                entity.ShowReply = "false";
                var values = new Dictionary<string, string>{
                   { "username", currentUser },
                   { "report", "true" },
                   { "block", block.ToString() },
                   { "device_id", devicePushId },
                };
                var content = new FormUrlEncodedContent(values);
                await App.globalClient.PatchAsync(App.HolidailyHost +
                    $"/{endpoint}/" + entity.Id + "/", content);
            }

            Utils.RefreshElement(container);
        }

        //async void ReportComment(object sender, EventArgs args)
        //{
        //    if (!isLoggedIn)
        //    {
        //        App.promptLogin(Navigation);
        //    }
        //    else
        //    {
        //        var comment = (sender as Label).BindingContext as Comment;
        //        var confirm = await DisplayAlert("Report Comment", $"Are you sure " +
        //            $"you'd like to report this comment by {comment.UserName}?", "Yes", "No");
        //        if (confirm)
        //        {
        //            comment.ShowReport = false;
        //            bool block = false;
        //            var confirmBlock = await DisplayAlert("Reported", $"Thank you for " +
        //                $"making Holidaily a safer place. Would you like to block " +
        //                $"future content from {comment.UserName}?", "Yes", "No");
        //            if (confirmBlock)
        //            {
        //                await DisplayAlert($"{comment.UserName} blocked",
        //                    $"{comment.UserName} has been blocked", "OK");
        //                comment.Content = "[blocked]";
        //                comment.UserName = "[blocked]";
        //                block = true;
        //            }
        //            else
        //            {
        //                comment.Content = "[reported]";
        //                comment.UserName = "[reported]";
        //            }
        //            comment.Avatar = "default_user_32.png";
        //            await viewModel.CommentStore.ReportComment(comment.Id, block.ToString());
        //        }
        //    }
        //}

        //private void OnItemTapped(object sender, ItemTappedEventArgs e)
        //{
        //    var comment = e.Item as Comment;
        //    var commentGroup = e.Group as CommentList;
        //    var commentIndexInGroup = commentGroup.IndexOf(comment);
        //    var entireList = (HolidayDetailList.ItemsSource as List<CommentList>).IndexOf(commentGroup);
        //    var commentId = comment.Id;
        //    Debug.WriteLine("Tapped comment id: " + commentId);
        //    // if entireList entireList == page, load more

        //}

        public void OnCommentSelected(object sender, SelectedItemChangedEventArgs args)
        {
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            //var item = args.SelectedItem as Comment;
        }

        protected async void RefreshPosts(object sender, EventArgs e)
        {
            HolidayPosts.Clear();
            HolidayPosts = await GetHolidayPosts();
            PostList.ItemsSource = HolidayPosts;
            PostList.EndRefresh();
        }

        private void RefreshPostsAndScroll(Comment comment, Post post, dynamic container)
        {

            post.ShowComments = true;

            if(post.Comments == null)
            {
                post.Comments = new ObservableCollection<Comment>()
                {
                    comment
                };
            }
            else
            {
                post.Comments.Insert(0, comment);
            }
            
            var index = ((IList)PostList.ItemsSource).IndexOf(post);
            try
            {

                PostList.ScrollTo(((IList)PostList.ItemsSource)[index], ScrollToPosition.Start, true);
            }
            catch
            {

            }

            Utils.RefreshElement(container);

        }

        async Task<ObservableCollection<Post>> GetHolidayPosts()
        {
            string url = $"{App.HolidailyHost}/posts/?holiday_id={viewModel.Holiday.Id}";

            if (isLoggedIn)
            {
                url = $"{url}&username={currentUser}";
            }

            var response = await App.globalClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic posts = responseJSON.results;
            string ShowDeleteVal;
            foreach (var p in posts)
            {
                string suffix = p.time_since_edit.ToString().Contains("now") ? "" : " ago";
                string TimeAgo = p.edited == null ? p.time_since : $"{p.time_since} (edited {p.time_since_edit}{suffix})";
                string postAuthor = p.user;
                if (String.Equals(postAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                {
                    ShowDeleteVal = "true";
                }
                else
                {
                    ShowDeleteVal = "false";
                }
                bool showReport = true;

                // If already blocked, reported or it's their own, don't allow another report
                if (p.blocked == true || p.reported == true ||
                    (String.Equals(postAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                    || p.deleted == true)
                {
                    showReport = false;
                }

                // Show user their own avatar, approved or not
                string avatar;
                if (p.user == App.GlobalUser.UserName && Settings.IsLoggedIn)
                {
                    avatar = App.GlobalUser.Avatar == null ? "default_user_128.png" : App.GlobalUser.Avatar;
                }
                else
                {
                    avatar = p.avatar == null ? "default_user_128.png" : p.avatar;
                }
                string image = p.image;
                bool isMediaVisible = string.IsNullOrEmpty(image) ? false : true;

                // post comments
                var comments = p.comments;
                ObservableCollection<Comment> PostComments = new ObservableCollection<Comment>();
                foreach (var comment in comments)
                {

                    string suffixComment = comment.time_since_edit.ToString().Contains("now") ? "" : " ago";
                    string TimeAgoComment = comment.edited == null ? comment.time_since :
                        $"{comment.time_since} (edited {comment.time_since_edit}{suffixComment})";
                    string commentUser = comment.user;
                    string avatarComment;
                    if (comment.UserName == App.GlobalUser.UserName && Settings.IsLoggedIn)
                    {
                        avatarComment = App.GlobalUser.Avatar;
                    }
                    else
                    {
                        avatarComment = comment.avatar == null ? "default_user_128.png" : comment.avatar;
                    }

                    string commentAuthor = comment.user;
                    string ShowDeleteComment;
                    if (String.Equals(commentAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                    {
                       
                        ShowDeleteComment = "true";
                    }
                    else
                    {

                        ShowDeleteComment = "false";
                    }
                    bool showReportComment = true;

                    // If already blocked, reported or it's their own, don't allow another report
                    if (comment.blocked == true || comment.reported == true ||
                        (String.Equals(commentAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                        || comment.deleted == true)
                    {
                        showReportComment = false;
                    }

                    dynamic replies = comment.replies;
                    

                    PostComments.Add(new Comment()
                    {
                        Id = comment.id,
                        Content = comment.content,
                        HolidayId = comment.holiday,
                        UserName = comment.user,
                        TimeSince = TimeAgoComment,
                        Avatar = avatarComment,
                        ShowEdit = ShowDeleteComment, // If you can delete, you can edit
                        ShowDelete = ShowDeleteComment,
                        ShowReport = showReportComment,

                        LikeImage = comment.liked == true ? "like_active.png" : "like_neutral.png",
                        Likes = comment.likes,
                        LikeTextColor = comment.liked == true ? Color.FromHex("4c96e8") : Color.FromHex("808080"),
                        LikeEnabled = true,
                        ShowReactions = comment.likes > 0 ? true : false,

                    });
                    // Append replies
                    foreach (var r in replies)
                    {

                        string avatarReply;
                        string replyAuthor = r.user;
                        if (r.UserName == App.GlobalUser.UserName && Settings.IsLoggedIn)
                        {
                            avatarReply = App.GlobalUser.Avatar;
                        }
                        else
                        {
                            avatarReply = r.avatar == null ? "default_user_128.png" : r.avatar;
                        }
                        string ShowDeleteReply;
                        if (String.Equals(replyAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                        {

                            ShowDeleteReply = "true";
                        }
                        else
                        {

                            ShowDeleteReply = "false";
                        }
                        bool showReportReply= true;

                        // If already blocked, reported or it's their own, don't allow another report
                        if (r.blocked == true || r.reported == true ||
                            (String.Equals(replyAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                            || r.deleted == true)
                        {
                            showReportReply = false;
                        }
                        Thickness paddingThickness = new Thickness(Convert.ToDouble(20), 0, 0, 0);

                        string suffixReply = r.time_since_edit.ToString().Contains("now") ? "" : " ago";
                        string TimeAgoReply = r.edited == null ? r.time_since :
                            $"{r.time_since} (edited {r.time_since_edit}{suffixReply})";

                        PostComments.Add(new Comment()
                        {
                            Id = r.id,
                            Content = r.content,
                            HolidayId = r.holiday,
                            UserName = replyAuthor,
                            TimeSince = TimeAgoReply,
                            Avatar = avatarReply,
                            ShowEdit = ShowDeleteReply, // If you can delete, you can edit
                            ShowDelete = ShowDeleteReply,
                            ShowReport = showReportReply,
                            ThreadPadding = paddingThickness,

                            LikeImage = r.liked == true ? "like_active.png" : "like_neutral.png",
                            Likes = r.likes,
                            LikeTextColor = r.liked == true ? Color.FromHex("4c96e8") : Color.FromHex("808080"),
                            LikeEnabled = true,
                            ShowReactions = r.likes > 0 ? true : false

                        });
                    }

                }

                HolidayPosts.Add(new Post()
                {
                    Id = p.id,
                    Content = p.content,
                    HolidayId = p.holiday,
                    UserName = p.user,
                    TimeSince = TimeAgo,
                    ShowEdit = ShowDeleteVal, // If you can delete, you can edit
                    ShowDelete = ShowDeleteVal,
                    ShowReport = showReport,
                    
                    Avatar = avatar,
                    Image = p.image,
                    ShowImage = isMediaVisible,
                    LikeImage = p.liked == true ? "like_active.png" : "like_neutral.png",
                    Likes = p.likes,
                    LikeTextColor = p.liked == true ? Color.FromHex("4c96e8") : Color.FromHex("808080"),
                    LikeEnabled = true,
                    ShowReactions = p.likes > 0 ? true : false,
                    ShowComments = PostComments.Count() > 0 ? true: false,
                    Comments = PostComments
                });


            }
            return await Task.FromResult(HolidayPosts);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Send(Application.Current, "UpdateToolbar", false);
            Debug.WriteLine("Holiday detail appearing");
            //if (viewModel.GroupedCommentList.Count == 0)
            //{
            //    //viewModel.LoadHolidayComments.Execute(null);

            //}



            try
            {
                viewModel.Holiday = await viewModel.HolidayStore.GetHolidayById(viewModel.HolidayId);
                HolidayImageSource.Source = viewModel.Holiday.HolidayImage;
                if (!string.IsNullOrEmpty(viewModel.Holiday.Description))
                {

                    Description.Text = viewModel.Holiday.Description;
                }
                else
                {
                    Description.Text = "This holiday has no information yet!";
                }

                HolidayDate.Text = viewModel.Holiday.TimeSince;

                TitleBar.Title = viewModel.Holiday.Name;
                CurrentVotes.Text = viewModel.Holiday.Votes.ToString() + " Celebrating!";

                if (isLoggedIn)
                {
                    UpVoteImage.Source = viewModel.Holiday.CelebrateStatus;
                    if(App.GlobalUser.Avatar != null)
                    {
                        PosterImage.Source = App.GlobalUser.Avatar;
                    }
                    
                }
                else
                {
                    PosterImage.Source = "default_user_128.png";
                }

                PostList.IsVisible = true;

                //if (viewModel.CommentLink != null)
                //{
                //    // TODO scroll to comment linked
                //    //HolidayDetailList.ScrollTo(viewModel.CommentLink, ScrollToPosition.MakeVisible, true);
                //    //Debug.WriteLine($"Index of comment {viewModel.GroupedCommentList.IndexOf(viewModel.CommentLink)}");
                //}

            }
            catch
            {
                await DisplayAlert("Error", "We couldn't fetch the data for this holiday", "OK");
                await Navigation.PopAsync();
            }
            finally
            {
                try
                {
                    if (HolidayPosts.Count == 0)
                    {
                        HolidayPosts = await GetHolidayPosts();
                    }

                    PostList.ItemsSource = HolidayPosts;
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"{ex}");
                }

            }

            // On login, refresh holiday celebration data
            MessagingCenter.Subscribe<LoginPage>(this, "UpdateHoliday", (sender) =>
            {
                UpdateHoliday();
                MessagingCenter.Unsubscribe<LoginPage>(this, "UpdateHoliday");
            });

            // On login, refresh comment data
            MessagingCenter.Subscribe<LoginPage>(this, "UpdatePosts", (sender) => {
                RefreshPosts(null, null);
                MessagingCenter.Unsubscribe<LoginPage>(this, "UpdatePosts");
            });

            MessagingCenter.Subscribe<NewCommentPopUp, Object[]>(this,
            "UpdateCommentInPlace", (sender, data) =>
            {
                var comment = ((Comment)data[0]);
                comment.Content = (string)data[1];
                comment.TimeSince = $"{(string)data[2]}";

                Utils.RefreshElement((data[3] as ContentView));

            });

            MessagingCenter.Subscribe<PostPage, Object[]>(this,
            "UpdatePostInPlace", (sender, data) =>
            {

                var post = ((Post)data[0]);
                post.Content = (string)data[1];
                post.TimeSince = $"{(string)data[2]}";
                string image = (string)data[4];
                post.ShowImage = string.IsNullOrEmpty(image) ? false : true;
                post.Image = image;

                Utils.RefreshElement((data[3] as ContentView));

            });



            MessagingCenter.Subscribe<NewCommentPopUp, Object[]>(this, "UpdateComments", (sender, data) =>
            {
                //RefreshPosts(null, null);
                //ScrollToFirstPost();
                RefreshPostsAndScroll((Comment)data[0], (Post)data[1], (dynamic)data[2]);
            });

            // Edit post and edit comment go to different places
            MessagingCenter.Subscribe<PostPage>(this, "RefreshPosts", (sender) =>
            {
                RefreshPosts(null, null);
                MessagingCenter.Unsubscribe<PostPage>(this, "RefreshPosts");
            });


            MessagingCenter.Subscribe<PostOptionsPopUp, Object[]>(this,
            "EditPost", (sender, data) =>
            {
                var container = (ContentView)data[1];
                if (data[0].GetType() == typeof(Post))
                {
                    
                    var post = (Post)data[0];
                    Navigation.PushModalAsync(
                       new NavigationPage(
                           new PostPage(viewModel.Holiday, post, container)
                       ));

                }
                else if (data[0].GetType() == typeof(Comment))
                {
                    var comment = (Comment)data[0];
                    Navigation.PushPopupAsync(new NewCommentPopUp(viewModel.Holiday, comment, edit: true, container: container));

                }

            });


            MessagingCenter.Subscribe<PostOptionsPopUp, Object[]>(this,
            "DeletePost", (sender, data) =>
            {
                var container = (ContentView)data[1];
                if (data[0].GetType() == typeof(Post))
                {

                    var post = (Post)data[0];
                    DeletePost(post: post, container: container);

                }
                else if (data[0].GetType() == typeof(Comment))
                {
                    var comment = (Comment)data[0];
                    DeletePost(comment: comment, container: container);

                }

            });


            MessagingCenter.Subscribe<PostOptionsPopUp, Object[]>(this,
            "ReportPost", (sender, data) =>
            {
                var container = (ContentView)data[1];
                if (data[0].GetType() == typeof(Post))
                {

                    var post = (Post)data[0];
                    ReportPost(post: post, container: container);

                }
                else if (data[0].GetType() == typeof(Comment))
                {
                    var comment = (Comment)data[0];
                    ReportPost(comment: comment, container: container);

                }

            });

            #if __IOS__
                MessagingCenter.Unsubscribe<PostPage, Post>(this, "AddPost");
            #endif

            //AdBanner.IsVisible = !isPremium;

        }

        async void ScrollToFirstPostAsync()
        {
            int numTries = 10;
            while (HolidayPosts.Count == 0)
            {
                numTries++;

                if (numTries >= 15)
                {
                    await DisplayAlert("Error", "Couldn't load posts", "OK");
                    return;
                }
                await Task.Delay(500);
            }
            PostList.ScrollTo(((IList)PostList.ItemsSource)[0], ScrollToPosition.Start, true);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<NewCommentPopUp, Object[]>(this, "UpdateComments");
            MessagingCenter.Unsubscribe<PostOptionsPopUp, Object[]>(this, "EditPost");
            MessagingCenter.Unsubscribe<PostOptionsPopUp, Object[]>(this, "DeletePost");
            MessagingCenter.Unsubscribe<PostOptionsPopUp, Object[]>(this, "ReportPost");
            MessagingCenter.Unsubscribe<NewCommentPopUp, Object[]>(this, "UpdateCommentInPlace");
            MessagingCenter.Unsubscribe<NewCommentPopUp, Object[]>(this, "UpdatePostInPlace");

            // This strange conditional + boolean is because Android has a huge
            // problem with onAppearing/OnDisappearing handlers when adding a
            // picture. They get desynced, so there's no reliable way to sub/
            // unsub to "AddPost" without the bool lock. So we only sub using
            // this lock if not subbed already instead of unsubbing on
            // disappear. This may be a good approach in the future actually.
            #if __ANDROID__
            if (!isAndroidAddPostSubscribed)
                {
                    isAndroidAddPostSubscribed = true;
            #endif

                    MessagingCenter.Subscribe<PostPage, Post>(this, "AddPost", (sender, post) =>
                    {
                        if (HolidayPosts.Count > 0)
                        {
                            HolidayPosts.Insert(0, post);
                            PostList.ScrollTo(((IList)PostList.ItemsSource)[0], ScrollToPosition.Start, true);
                        }
                        else
                        {
                            //ScrollToFirstPostAsync();
                            HolidayPosts = new ObservableCollection<Post>(){
                                post
                                };
                            PostList.ItemsSource = HolidayPosts;
                            PostList.ScrollTo(((IList)PostList.ItemsSource)[0], ScrollToPosition.Start, true);
                        }
                    });
            #if __ANDROID__
                }
            #endif
        }



        public void Share(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            var holiday = viewModel.Holiday;
            var holidayName = holiday.Name;
            var holidayLink = App.HolidailyHost + "/holiday?id=" + holiday.Id;
            string blurb = $"{holidayName}! {holiday.Blurb}\nCheck it out on Holidaily!";

            if (!CrossShare.IsSupported)
                return;

            CrossShare.Current.Share(new ShareMessage
            {
                Title = holidayName,
                Text = blurb,
                Url = holidayLink
            });

            this.IsEnabled = true;

        }

        private bool coolDown;
        public async void AddPost(object sender, EventArgs args)
        {
            if (coolDown)
                return;
            coolDown = true;
            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
                coolDown = false;
                return;
            }
            await Navigation.PushModalAsync(new NavigationPage(new PostPage(holiday: viewModel.Holiday)));
            await Task.Delay(1000);
            coolDown = false;
        }

        async void UpVote(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            Utils.Vibrate();
            string VotesString = CurrentVotes.Text;
            int votesInt = Int32.Parse(VotesString.Split(null)[0]);
            int newVotesInt = Int32.Parse(VotesString.Split(null)[0]);
            var UpVoteImageFile = UpVoteImage.Source as FileImageSource;
            var UpVoteIcon = UpVoteImageFile.File;


            if (!isLoggedIn)
            {
                this.IsEnabled = false;
                App.promptLogin(Navigation);
                this.IsEnabled = true;
            }
            else
            {

                if (UpVoteIcon == "celebrate_active.png")
                {
                    // Undo

                    newVotesInt -= 1;
                    CurrentVotes.Text = newVotesInt.ToString() + " Celebrating!";
                    UpVoteImage.Source = "celebrate.png";
                    await UpVoteImage.ScaleTo(2, 50);
                    await UpVoteImage.ScaleTo(1, 50);
                    await Services.GlobalServices.VoteHoliday(viewModel.HolidayId, "3");


                    Object[] values = { viewModel.Holiday.Name, false, newVotesInt.ToString() };
                    MessagingCenter.Send(this, "UpdateCelebrateStatus", values);
                }
                else
                {
                    // Only allow if user hasnt already downvoted
                    newVotesInt += 1;
                    if (newVotesInt <= votesInt + 1 && newVotesInt >= votesInt - 1)
                    {
                        CurrentVotes.Text = newVotesInt.ToString() + " Celebrating!";
                        UpVoteImage.Source = "celebrate_active.png";
                        await UpVoteImage.ScaleTo(2, 50);
                        await UpVoteImage.ScaleTo(1, 50);
                        await Services.GlobalServices.VoteHoliday(viewModel.HolidayId, "1");


                    Object[] values = { viewModel.Holiday.Name, true, newVotesInt.ToString() };
                        MessagingCenter.Send(this, "UpdateCelebrateStatus", values);
                }
                else
                {
                    newVotesInt -= 2;
                    CurrentVotes.Text = newVotesInt.ToString() + " Celebrating!";
                    UpVoteImage.Source = "celebrate.png";
                    await UpVoteImage.ScaleTo(2, 50);
                    await UpVoteImage.ScaleTo(1, 50);
                    await Services.GlobalServices.VoteHoliday(viewModel.HolidayId, "5");

    
                    Object[] values = { viewModel.Holiday.Name, false, newVotesInt.ToString() };
                    MessagingCenter.Send(this, "UpdateCelebrateStatus", values);

                }

            }

            }
            this.IsEnabled = true;

        }


        async void DownVoteComment(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            //var voteStatus = (sender as Image).Source;
            var item = (sender as ContentView).BindingContext as Comment;
            string commentId = item.Id;

            Utils.Vibrate();


            await (sender as ContentView).ScaleTo(2, 50);
            await (sender as ContentView).ScaleTo(1, 50);

            int CurrentVotes = item.Votes;

            if (!isLoggedIn)
            {
                this.IsEnabled = false;
                App.promptLogin(Navigation);
                this.IsEnabled = true;
            }
            else
            {

                if (item.UpVoteStatus == "up_active.png")
                {
                    item.Votes -= 2;
                    item.UpVoteStatus = "up.png";
                    item.DownVoteStatus = "down_active.png";
                    await viewModel.CommentStore.VoteComment(commentId, "5");
                }
                else
                {
                    if (item.DownVoteStatus == "down_active.png")
                    {
                        // Undo
                        item.Votes += 1;
                        item.DownVoteStatus = "down.png";
                        await viewModel.CommentStore.VoteComment(commentId, "2");
                    }
                    else
                    {
                        // Only allow if user hasnt already downvoted
                        int newVotes = item.Votes - 1;
                        if (newVotes <= item.Votes + 1 && newVotes >= item.Votes - 1)
                        {
                            item.Votes -= 1;
                            item.DownVoteStatus = "down_active.png";
                            await viewModel.CommentStore.VoteComment(commentId, "0");
                        }
                        else
                        {
                            // Undo
                            item.Votes += 2;
                            item.DownVoteStatus = "down.png";
                            await viewModel.CommentStore.VoteComment(commentId,"4");
                        }
                    }
                }

            }
            this.IsEnabled = true;


        }

        async void UpVoteComment(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            var item = (sender as ContentView).BindingContext as Comment;
            string commentId = item.Id;

            Utils.Vibrate();

            await (sender as ContentView).ScaleTo(2, 50);
            await (sender as ContentView).ScaleTo(1, 50);

            int CurrentVotes = item.Votes;

            if (!isLoggedIn)
            {
                this.IsEnabled = false;
                App.promptLogin(Navigation);
                this.IsEnabled = true;
            }
            else
            {

                if (item.DownVoteStatus == "down_active.png")
                {
                    item.Votes += 2;
                    item.DownVoteStatus = "down.png";
                    item.UpVoteStatus = "up_active.png";
                    await viewModel.CommentStore.VoteComment(commentId, "4");
                }
                else
                {
                    if (item.UpVoteStatus == "up_active.png")
                    {
                        // Undo

                        item.Votes -= 1;
                        item.UpVoteStatus = "up.png";
                        await viewModel.CommentStore.VoteComment(commentId, "3");
                    }
                    else
                    {
                        // Only allow if user hasnt already downvoted
                        int newVotes = item.Votes + 1;
                        if (newVotes <= item.Votes + 1 && newVotes >= item.Votes - 1)
                        {
                            item.Votes += 1;
                            item.UpVoteStatus = "up_active.png";
                            await viewModel.CommentStore.VoteComment(commentId, "1");
                        }
                        else
                        {

                            item.Votes -= 2;
                            item.UpVoteStatus = "up.png";
                            await viewModel.CommentStore.VoteComment(commentId, "5");
                        }

                    }


                }
            }
            this.IsEnabled = true;

        }

        public async void PreviewImage(object sender, EventArgs args)
        {
            try
            {
                new PhotoBrowser
                {
                    Photos = new List<Photo>
                {
                    new Photo
                    {
                        URL = $"{viewModel.Holiday.HolidayImage}",
                        Title = $"{viewModel.Holiday.Name}"
                    }
                }
                }.Show();
            }
            catch(Exception e)
            {
                await DisplayAlert("Ouch!", "Sorry, we couldn't load this" +
                    " image at the moment", "OK");
                Debug.WriteLine($"{e}");
            }

        }
        public async void OpenOptions(object sender, EventArgs args)
        {
            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
                return;
            }
            Post post = (sender as ContentView).BindingContext as Post;
            try
            {
                await Navigation.PushPopupAsync(new PostOptionsPopUp(post: post, container: (sender as ContentView)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }

        }

        public async void OpenCommentOptions(object sender, EventArgs args)
        {

            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
                return;
            }
            Comment comment = (sender as ContentView).BindingContext as Comment;
            try
            {
                await Navigation.PushPopupAsync(new PostOptionsPopUp(comment: comment, container: (sender as ContentView)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }

        }

        public async void ViewPostimage(object sender, EventArgs args)
        {
            Post post = (sender as CachedImage).BindingContext as Post;
            try
            {
                new PhotoBrowser
                {
                    Photos = new List<Photo>
                {
                    new Photo
                    {
                        URL = $"{post.Image}",
                        Title = $"{post.UserName}'s Image"
                    }
                }
                }.Show();
            }
            catch (Exception e)
            {
                await DisplayAlert("Ouch!", "Sorry, we couldn't load this" +
                    " image at the moment", "OK");
                Debug.WriteLine($"{e}");
            }

        }

    }
}
