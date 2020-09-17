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
        public HolidayDetailPage(HolidayDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = this.viewModel = viewModel;
            HolidayPosts = new ObservableCollection<Post>();
            //NavigationPage.SetHasNavigationBar(this, false);
            // Remove when reply button added
            //PostList.ItemSelected += OnCommentSelected;

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
            var comment = new Comment();
            try
            {
                comment = (sender as ContentView).BindingContext as Comment;
            }
            catch
            {
                comment = (sender as Label).BindingContext as Comment;
            }
            if (comment.Content == "[deleted]" || comment.Content == "[blocked]" || comment.Content == "[reported]")
                return;
            string UserName = comment.UserName;
            await Navigation.PushAsync(new UserPage(user: null, userName: UserName));
        }

        async void DeletePost(Post post=null, Comment comment=null)
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
                            entity.ShowReply = "False";
                            entity.ShowDelete = "False";
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


        //async void EditPost(object sender, EventArgs args)
        //{
           
        //    var post = (sender as Label).BindingContext as Post;
        //    await Navigation.PushModalAsync(new NavigationPage(new PostPage(viewModel.Holiday, post)));
            
        //}

        async void LikePost(object sender, EventArgs args)
        {

            Utils.Vibrate();
            Post post = (sender as StackLayout).BindingContext as Post;


            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
                return;
            }

            if (!post.LikeEnabled)
                return;
            post.LikeEnabled = false;
            bool isLiked = post.LikeImage == "like_neutral.png" ? true : false;


            post.LikeImage = isLiked == false ? "like_neutral.png" : "like_active.png";
            post.LikeTextColor = isLiked == false ? Color.FromHex("808080"): Color.FromHex("4c96e8");

            if (isLiked)
            {
                await (sender as StackLayout).ScaleTo(1.5, 50);
                await (sender as StackLayout).ScaleTo(1, 50);
                post.Likes += 1;
            }
            else
            {
                post.Likes -= 1;
            }

            post.ShowReactions = post.Likes > 0 ? true : false;
            // Need to update height
            try
            {
                // Ensure that Listview has CachingStrategy="RecycleElement"
                // Or you will get tons of lag as updates compound

                // First way
                //var stack = (sender as StackLayout);
                //var grid = (Grid)stack.Parent;
                //var stack2 = (StackLayout)grid.Parent;
                //var frame = (CustomFrame)stack2.Parent;
                //var cv = (ContentView)frame.Parent;
                //var viewcell = (ViewCell)cv.Parent;

                // Second Way
                //var viewcell = (ViewCell)(sender as StackLayout).Parent.Parent.Parent.Parent.Parent;

                // Third Way
                var element = (sender as StackLayout).Parent;
                while (element.GetType() != typeof(ViewCell))
                {
                    element = element.Parent;
                }
                ((ViewCell)element).ForceUpdateSize();

            }
            catch(Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }


            try
            {
                var values = new Dictionary<string, string>{
                   { "username", currentUser },
                   { "device_id", devicePushId },
                   { "like", isLiked.ToString() },
                };
                var content = new FormUrlEncodedContent(values);
                await App.globalClient.PatchAsync(App.HolidailyHost +
                    "/posts/" + post.Id + "/", content);


            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Could not like post: {ex}");
            }
            finally
            {
                await Task.Delay(1000);
                post.LikeEnabled = true;
                //LikeContainer.IsEnabled = true;
            }
        }

        async void OnReplyTapped(object sender, EventArgs args)
        {

            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
            }
            else
            {
                
               Post post = (sender as StackLayout).BindingContext as Post;
                await Navigation.PushPopupAsync(new NewCommentPopUp(
                    viewModel.Holiday, entity: post, reply: true));
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

                await Navigation.PushPopupAsync(new NewCommentPopUp(
                    viewModel.Holiday, entity: comment, reply: true));
            }
        }



        async void ReportPost(Post post = null, Comment comment = null)
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
        }

        async void ReportComment(object sender, EventArgs args)
        {
            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
            }
            else
            {
                var comment = (sender as Label).BindingContext as Comment;
                var confirm = await DisplayAlert("Report Comment", $"Are you sure " +
                    $"you'd like to report this comment by {comment.UserName}?", "Yes", "No");
                if (confirm)
                {
                    comment.ShowReport = false;
                    bool block = false;
                    var confirmBlock = await DisplayAlert("Reported", $"Thank you for " +
                        $"making Holidaily a safer place. Would you like to block " +
                        $"future content from {comment.UserName}?", "Yes", "No");
                    if (confirmBlock)
                    {
                        await DisplayAlert($"{comment.UserName} blocked",
                            $"{comment.UserName} has been blocked", "OK");
                        comment.Content = "[blocked]";
                        comment.UserName = "[blocked]";
                        block = true;
                    }
                    else
                    {
                        comment.Content = "[reported]";
                        comment.UserName = "[reported]";
                    }
                    comment.Avatar = "default_user_32.png";
                    await viewModel.CommentStore.ReportComment(comment.Id, block.ToString());
                }
            }
        }

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

        //async Task<ObservableCollection<Comment>> GetPostComments(int PostId)
        //{
        //    ObservableCollection<Comment> PostComments = new ObservableCollection<Comment>();
        //    string url = $"{App.HolidailyHost}/comments/?post={PostId}";

        //    if (isLoggedIn)
        //    {
        //        url = $"{url}&username={currentUser}";
        //    }

        //    var response = await App.globalClient.GetAsync(url);
        //    var responseString = await response.Content.ReadAsStringAsync();
        //    dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
        //    dynamic posts = responseJSON.results;

        //    foreach (var p in posts)
        //    {
        //        PostComments.Add(new Comment()
        //        {
        //            Content = p.content
        //        });
        //    }
        //    return await Task.FromResult(PostComments);
        //}

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

            string ShowReplyVal;
            string ShowDeleteVal;
            foreach (var p in posts)
            {
                string suffix = p.time_since_edit.ToString().Contains("now") ? "" : " ago";
                string TimeAgo = p.edited == null ? p.time_since : $"{p.time_since} (edited {p.time_since_edit}{suffix})";
                string postAuthor = p.user;
                if (String.Equals(postAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                {
                    ShowReplyVal = "false";
                    ShowDeleteVal = "true";
                }
                else
                {
                    ShowReplyVal = "true";
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

                    string TimeAgoComment = comment.edited == null ? comment.time_since : $"{comment.time_since} (edited {comment.edited})";
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

                    });
                    // Append replies
                    foreach (var r in replies)
                    {
                        string replyAuthor = r.user;
                        string ShowDeleteReply;
                        if (String.Equals(commentAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
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
                            (String.Equals(commentAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                            || r.deleted == true)
                        {
                            showReportReply = false;
                        }
                        Thickness paddingThickness = new Thickness(Convert.ToDouble(20), 10, 10, 10);
                        PostComments.Add(new Comment()
                        {
                            Id = r.id,
                            Content = r.content,
                            HolidayId = r.holiday,
                            UserName = r.user,
                            TimeSince = TimeAgoComment,
                            Avatar = avatarComment,
                            ShowEdit = ShowDeleteReply, // If you can delete, you can edit
                            ShowDelete = ShowDeleteReply,
                            ShowReport = showReportReply,
                            ThreadPadding = paddingThickness

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
                    ShowReply = ShowReplyVal,
                    ShowEdit = ShowDeleteVal, // If you can delete, you can edit
                    ShowDelete = ShowDeleteVal,
                    ShowReport = showReport,
                    Likes = p.likes,
                    Avatar = avatar,
                    Image = p.image,
                    ShowImage = isMediaVisible,
                    LikeImage = p.liked == true ? "like_active.png" : "like_neutral.png",
                    LikeTextColor = p.liked == true ? Color.FromHex("4c96e8") : Color.FromHex("808080"),
                    ShowReactions = p.likes > 0 ? true : false,
                    LikeEnabled = true,
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

            //if (viewModel.GroupedCommentList.Count == 0)
            //{
            //    //viewModel.LoadHolidayComments.Execute(null);

            //}

            if(HolidayPosts.Count == 0)
            {
                Debug.WriteLine("Refreshing");
                HolidayPosts = await GetHolidayPosts();
            }

            PostList.ItemsSource = HolidayPosts;



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

                //this.Title = viewModel.Holiday.Name;
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

                if (viewModel.CommentLink != null)
                {
                    // TODO scroll to comment linked
                    //HolidayDetailList.ScrollTo(viewModel.CommentLink, ScrollToPosition.MakeVisible, true);
                    //Debug.WriteLine($"Index of comment {viewModel.GroupedCommentList.IndexOf(viewModel.CommentLink)}");
                }

            }
            catch
            {
                await DisplayAlert("Error", "We couldn't fetch the data for this holiday", "OK");
                await Navigation.PopAsync();
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



            MessagingCenter.Subscribe<NewCommentPopUp>(this, "UpdateComments", (sender) =>
            {
                Debug.WriteLine("Refreshing comments");
                viewModel.ExecuteLoadCommentsCommand();
            });

            MessagingCenter.Subscribe<PostPage>(this, "RefreshPosts", (sender) =>
            {
                RefreshPosts(null, null);
                MessagingCenter.Unsubscribe<PostPage>(this, "RefreshPosts");
            });

            MessagingCenter.Subscribe<PostOptionsPopUp, Post>(this, "EditPost", (sender, entity) =>
            {
                Navigation.PushModalAsync(
                           new NavigationPage(
                               new PostPage(viewModel.Holiday, entity)
                           ));
            });

            MessagingCenter.Subscribe<PostOptionsPopUp, Comment>(this, "EditPost", (sender, entity) =>
            {
               Navigation.PushPopupAsync(new NewCommentPopUp(viewModel.Holiday, entity, edit: true));
            });

            MessagingCenter.Subscribe<PostOptionsPopUp, Post>(this, "DeletePost", (sender, entity) =>
            {
                DeletePost(post: entity);
            });
            MessagingCenter.Subscribe<PostOptionsPopUp, Comment>(this, "DeletePost", (sender, entity) =>
            {
                DeletePost(comment: entity);
            });

            MessagingCenter.Subscribe<PostOptionsPopUp, Post>(this, "ReportPost", (sender, entity) =>
            {
                ReportPost(post: entity);
            });
            MessagingCenter.Subscribe<PostOptionsPopUp, Comment>(this, "ReportPost", (sender, entity) =>
            {
                ReportPost(comment: entity);
            });

            async void ScrollToPost()
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
            MessagingCenter.Subscribe<PostPage, Post>(this, "AddPost", (sender, post) =>
            {

                if(HolidayPosts.Count > 0)
                {
                    HolidayPosts.Insert(0, post);
                    PostList.ScrollTo(((IList)PostList.ItemsSource)[0], ScrollToPosition.Start, true);
                }
                else
                {
                    ScrollToPost();
                }
                MessagingCenter.Unsubscribe<PostPage, Post>(this, "AddPost");
            });

            AdBanner.IsVisible = !isPremium;

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<NewCommentPopUp>(this, "UpdateComments");
            MessagingCenter.Unsubscribe<PostOptionsPopUp, Post>(this, "EditPost");
            MessagingCenter.Unsubscribe<PostOptionsPopUp, Post>(this, "DeletePost");
            MessagingCenter.Unsubscribe<PostOptionsPopUp, Post>(this, "ReportPost");

            MessagingCenter.Unsubscribe<PostOptionsPopUp, Comment>(this, "EditPost");
            MessagingCenter.Unsubscribe<PostOptionsPopUp, Comment>(this, "DeletePost");
            MessagingCenter.Unsubscribe<PostOptionsPopUp, Comment>(this, "ReportPost");
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

        public async void AddPost(object sender, EventArgs args)
        {
            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
                return;
            }
            await Navigation.PushModalAsync(new NavigationPage(new PostPage(holiday: viewModel.Holiday)));

        }

        async void AddComment(object sender, EventArgs args)
        {
            
            this.IsEnabled = false;
            bool allowed = Time.ActiveHoliday(viewModel.Holiday.TimeSince);
            if (allowed)
            {
                if (!isLoggedIn)
                {
                    App.promptLogin(Navigation);
                }
                else
                {
                    this.IsEnabled = false;
                    await Navigation.PushPopupAsync(new NewCommentPopUp(viewModel.Holiday));
                    this.IsEnabled = true;
                }
            }
            else
            {
                await DisplayAlert("Sorry!", "We currently restrict new comments to holidays in the past week.", "OK");
            }
            this.IsEnabled = true;

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
                    await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, "3");


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
                        await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, "1");


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
                    await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, "5");

    
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
            Post post = (sender as ContentView).BindingContext as Post;
            try
            {
                await Navigation.PushPopupAsync(new PostOptionsPopUp(post: post));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }

        }

        public async void OpenCommentOptions(object sender, EventArgs args)
        {
     
          
            Comment comment = (sender as ContentView).BindingContext as Comment;
            try
            {
                await Navigation.PushPopupAsync(new PostOptionsPopUp(comment: comment));
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
