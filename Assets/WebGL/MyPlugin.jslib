var MyPlugin = {
    IsMobile: function()
     {
         return UnityLoader.SystemInfo.mobile;
     },

    GoFullscreen: function()
    {

        var viewFullScreen = document.getElementById('#canvas');

        var orientation = (screen.orientation || {}).type || screen.mozOrientation || screen.msOrientation;

        var ActivateFullscreen = function()
        {
            if(orientation == "landscape-primary"){
                if (viewFullScreen.requestFullscreen) /* API spec */
                {  
                    viewFullScreen.requestFullscreen();
                    screen.orientation.lock("landscape-primary");
                }
                else if (viewFullScreen.mozRequestFullScreen) /* Firefox */
                {
                    viewFullScreen.mozRequestFullScreen();
                    screen.mozLockOrientation.lock("landscape-primary");
                }
                else if (viewFullScreen.webkitRequestFullscreen) /* Chrome, Safari and Opera */
                {  
                    viewFullScreen.webkitRequestFullscreen();
                    screen.orientation.lock("landscape-primary");
                }
                else if (viewFullScreen.msRequestFullscreen) /* IE/Edge */
                {  
                    viewFullScreen.msRequestFullscreen();
                    screen.msLockOrientation.lock("landscape-primary");
                }
                viewFullScreen.removeEventListener('touchend', ActivateFullscreen);    
            }
        }
        viewFullScreen.addEventListener('touchend', ActivateFullscreen, false);
    },

    CheckOrientation: function(){
        var orientation = (screen.orientation || {}).type || screen.mozOrientation || screen.msOrientation;

        if(orientation == "landscape-primary")
        {
            return true;
        }
        else
        {
            return false;
        }
    },
};

mergeInto(LibraryManager.library, MyPlugin);  