using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Galaxy_Buds_Client.ui;

namespace Galaxy_Buds_Client.transition
{
	public partial class PageTransition : UserControl
	{
		Stack<UserControl> pages = new Stack<UserControl>();

        public UserControl CurrentPage { get; set; }
        public String CurrentPageName { get; private set; }

        public static readonly DependencyProperty TransitionTypeProperty = DependencyProperty.Register("TransitionType",
			typeof(PageTransitionType),
			typeof(PageTransition), new PropertyMetadata(PageTransitionType.SlideAndFade));

		public PageTransitionType TransitionType
		{
			get
			{
				return (PageTransitionType)GetValue(TransitionTypeProperty);
			}
			set 
			{
				SetValue(TransitionTypeProperty, value);
			}
		}

		public PageTransition()
		{
            CurrentPageName = "NULL";
			InitializeComponent();
		}		
		
		public void ShowPage(UserControl newPage)
		{			
			pages.Push(newPage);

			Task.Factory.StartNew(() => ShowNewPage());
		}

		void ShowNewPage()
		{
            try
            {
                Dispatcher.Invoke((Action) delegate
                {
                    if (contentPresenter.Content != null)
                    {
                        UserControl oldPage = contentPresenter.Content as UserControl;

                        if (oldPage != null)
                        {
                            oldPage.Loaded -= newPage_Loaded;

                            UnloadPage(oldPage);
                        }
                    }
                    else
                    {
                        ShowNextPage();
                    }

                });
            }
			catch(TaskCanceledException){}
        }

		void ShowNextPage()
		{
			UserControl newPage = pages.Pop();

			newPage.Loaded += newPage_Loaded;

			contentPresenter.Content = newPage;
		}

		void UnloadPage(UserControl page)
		{
			Storyboard hidePage = (Resources[string.Format("{0}Out", TransitionType.ToString())] as Storyboard).Clone();

			hidePage.Completed += hidePage_Completed;

			hidePage.Begin(contentPresenter);
		}

		void newPage_Loaded(object sender, RoutedEventArgs e)
		{
			Storyboard showNewPage = Resources[string.Format("{0}In", TransitionType.ToString())] as Storyboard;

			showNewPage.Begin(contentPresenter);

            if (CurrentPage != null && 
                CurrentPage.GetType().IsSubclassOf(typeof(BasePage)))
            {
                ((BasePage)CurrentPage).OnPageHidden();
            }
    
            CurrentPage = sender as UserControl;
            CurrentPageName = CurrentPage.GetType().Name;

            if (CurrentPage.GetType().IsSubclassOf(typeof(BasePage)))
            {
                ((BasePage)CurrentPage).OnPageShown();
            }
		}		

		void hidePage_Completed(object sender, EventArgs e)
		{
			contentPresenter.Content = null;

			ShowNextPage();
		}		
	}
}
