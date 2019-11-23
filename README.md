# :pencil: (Use Case) Custom Render Window
---------

:pushpin: Overview
---------
Custom Render Window is part of cargo system which was developed,
with focus on improve game designers workflow with game objects (trucks, cargo).
Here is **Truck Preview Renderer** which I would like to share with you.

![CargoArchitecture](https://user-images.githubusercontent.com/14979589/69476392-01ab6080-0de2-11ea-83c8-97a96a7c5eb1.PNG)

**Repository contain example rendering window which will be helpful as reference, for easy implement your own features.**

:bulb: Idea
---------
In trucks oriented game we need develop render window tool which will able to customize cargo on trucks.
Tool will be used by designers in to improve asset workflow and avoid to work with seperate objects in scenes.

* Potential tool input
    * Cargo pack prefab with specific cargo as childs
    * Mesh of truck to render

:white_check_mark: Goals
---------
* Rendering truck & cargo
* Easy switching between trucks and cargo
* Cargo transform settings
* Easy use for various types of cargo
* Intuitive movement behaviour of obj in render area
* Save data to structures and use in game

:fire: Result
---------

:poop: What issues was handled 
---------
* Can't modify truck prefab.
  * Use separate template like mesh obj instead of truck prefab  
* Can't modify specific cargo obj from prefab hrierarchy.
  * Read data from cargo obj then just work with data from own structure
* Keep data in specific structure and dictionary is not serializable.
  * Create own serializable dictionary in unity and store into scriptable obj

:package: Package to download
---------
* Window editor renderer ready for build your own tool.
   * Render Obj
   * Error Handler
   * Obj rotation & zooming
   
![RenderWindow](https://user-images.githubusercontent.com/14979589/69479562-4f868f80-0e07-11ea-82ab-56127a493b53.PNG)
