### Intro
Memotopia is built on **Unity 2019.3.12f1**. Model-View-Controller (MVC) is the main design pattern used in the project when implementing solutions that involve communication with Backend and Unity.
However game logic and other in-game implementations are mainly developed with Object Oriented Programming in mind (OOP).
Moreover in the project can be found other patterns such as Scriptable Objects, Singletons and Interfaces.

### Scenes structure
**LoginScene** This is the first scene which is activated as soon as the game opens.This scene contains a simple video animation of the logo, then automatically opens the Login scene.

**SplashScene** This stage manages the implementation of authentication and includes three main views/screens:
* Login Screen
* SignupScreen
* ForgotPasswordScreen

In this scene, we also create the LoadinScreen view which is then preserved in all subsequent scenes and used when necessary. From this scene, once authentication is complete, we proceed to the Game Menu scene.

**GameMenu** This scene has most of the views and is the central scene that ties most of the other scenes together. In this scene we will find the following views:
* The SideMenu - Right panel that allows us to move on different screens in the same scene
* Home - The initial screen that opens upon entering this scene.
* Statistics - This is the view which shows the main statistics and has buttons for opening more detailed statistics as well.
* Achievements - Is the display that contains all the achievements that are completed and the ones that need to be achieved.
* Global Score - Is a view which can be opened from the acvhivemnts screen and shows a list of the nine best scores and ours.
* Training - It's the screen that allows us to choose between the three games we want to play.
* Account - This is the screen where we can change our account such as selecting our avatar and changing the username.We can open this view from the SidePanel screen
* Challenge - This is the initial view that is opened when we want to start a challenge game. Here we will choose the player that we want to challenge
* Tips - Is the screen that has some writing tips and suggestions to empower the brain 
* Videos - As you can imagine here we have a couple videos from Idriz 
* Payments - Is the screen that handles subscriptions 

In this scene we also have to pop-up
* RestrictionsPanel 
* AchievementPopup

It's worth mentioning that in this scene we have a game object called `MenuView` in which most of the scripts are attached to

**LevelsScene** It is the common scene for the three games which allows us to choose the level for the respective game.

**PairsScene** This is the scene that contains the game called Pair.

**FlashScene** This is the scene that contains the game called Flash.

**BoxesScene** This is the scene that contains the game called Boxes.

**BoxesSceneIpad** It is the scene that contains Box game, but for iPad. This one was created as it was difficult to make this game in one scene that will handle all screen sizes. Propably it will need a bit more work and this scne should be merged with `BoxesScene`

### Main scripts and scripts folder structure
**Scripts** folder structure is constructed with MVC in mind.
Under the Scripts folder there is a folder named **Constants** and in this folder are located different scripts that contain hardcoded variables or server driven variables 
that are used throughout the project. In `APIConstant.cs` we have a `LOCAL_BASE_URL` variable that defines which Backend environment should we use (local, development or production) and 
this is one of the first steps that need to be defined when building or testing the project. Other scripts and variables in this folder I assume are self-explanatory.

**Controllers** folder contains all API controller scripts that are used to add and get data from the backend. So scripts that are found here are responsible for
the workflow of the data in both unity and server.

**GamesScripts** folder is populated with scripts that handle game logic. In the **GamesScripts/General** folder are located scripts that are not specific for a particular
game such as `GamesScript.cs` that handles logic that is common in all games. In this script we initialize games, show and close in game panels (such as pause, win,
game over etc) etc. Other important scripts inside GamesScripts folder are `BoxesManager.cs`, `FlashManager.cs` and `PairsManager.cs` those scripts are responsible for
the logics of each game separately.

Under **Models** folder are located all scripts that handle data logic (data structure).

**Payment** folder contains three scripts that handle the subscription payment process. Payment logic is implemented on `MyIAPManager.cs`, this script is built with
[**Unity in App Purchase**](https://learn.unity.com/tutorial/unity-iap?uv=5.x#) and follows official Unity documentation.` PurchaseButton.cs` is responsible for the purchase button behaviour and look. Moreover under
**Scripts/UnityPurchasing/generated** there are two files that are generated by Unity after implementing Receipt Validation Obfuscator. This one is implemented by going
**Windows - Unity IAP - Receipt Validation Obfuscator** then we have pasted android public key and then obfuscator the key. Therefore we have two folders that have
Payments implementation in the project.

Under the **ScriptableObjects** folder we have one script that is responsible for creating achievements objects. This script is used as the base class and from this
one we can create different achievements based on different inputs on the object.

In **Utilities** we use scripts that help in one or the other way to implement different features.

In **Views** we have all the scripts related to UI, such as scripts that show popups, scripts of different views such as statistics, achievements, authentications etc.  
