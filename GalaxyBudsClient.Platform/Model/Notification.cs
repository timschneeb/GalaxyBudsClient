namespace GalaxyBudsClient.Platform.Model;

/**
 * <param name="Title">The title of the notification</param>
 * <param name="Content">The text content of the notification</param>
 * <param name="AppName">A user-friendly name of the application that sent the notification</param>
 * <param name="AppId">An unique identifier of the sender application</param>
 */
public record Notification(
    string Title, 
    string Content, 
    string AppName, 
    string AppId
);