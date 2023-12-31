# EdgeFinder_Tool

> ⚠️ WARNING
> This tool is still a **WIP**. But definitely feel free to play around with it in your projects and modify it further if it suits your needs!

![image](https://github.com/jbakeacake/EdgeFinder_Tool/assets/34492737/84df0f74-20d8-4bb1-9fce-4c17d7cf82bd)

## Changelog

You can find the changelog [here](CHANGELOG.md).

## Getting Started

### What is it?
This is a simple tool to help generate and map DreamTeck Splines to any uniquely defined edges by their associated vertex colors.

### Why would I use this Tool?
Out of the box, most Spline tools require manual placement of control points along some plane or object. This tool covers one use case where a user might want 
to map a spline along a certain edge of their mesh. Additionally, it adds an interface for querying/collision detection for the spline at runtime by generating a small mesh collider around the outline
of the spline.

### Dependencies (Packages)
- [DreamTeck Splines](https://assetstore.unity.com/packages/tools/utilities/dreamteck-splines-61926): The spline implementation we're going to use to generate and map our splines. ⚠️ **Required!** ⚠️
- [Naughty Attributes](https://assetstore.unity.com/packages/tools/utilities/naughtyattributes-129996): A (mostly) optional editor asset we use to generate in-editor/inspector buttons for this tool. Feel free to use your own in-editor tools however.
- [Probuilder](https://unity.com/features/probuilder): This comes out of the box on Unity's asset registry. We use this to help select edge loops and paint vertex-colors onto those edges.

### Why DreamTeck Splines?
Honest answer, it was the first Spline package recommended to me 🤷‍♂️ but more importantly: it's free! Also, DreamTeck also comes along with a whole slew of additional tools that let you easily use their splines. 
Specifically, they provide [Spline Users](https://dreamteck-splines.netlify.app/#/./pages/using_splines/using_splines) which allow you to apply their Spline's in a whole 'nother slew of ways.

See their full documentation [here](https://dreamteck-splines.netlify.app/#/).

### Installation

Installation is little bit tricky with these dependencies. For now, you can just download/copy the `EdgeFinder.cs` and `DebugUtilities.cs` files and paste them into your project.

---

### How do I use this thing?

> ℹ️ You can skip steps 0 & 1 if you've already marked your vertex colors in a separate program (like Blender).

#### Step 0. Probuilderize!
Probuilder allows you to ![image](https://github.com/jbakeacake/EdgeFinder_Tool/assets/34492737/626478b4-60da-49c9-8231-9bd9760cd6f9) almost any gameobject you want. This step is required so we can perform some
edge selects to mark our edges with vertex colors.

#### Step 1. Mark your Edges!
Before we mess around with the `EdgeFinder`, we first want to make sure we've gone through our mesh, and marked the appropriate edges we want to generate a spline for with a unique color. To test things
out, I've gone ahead and created a simple pipe using Probuilder's tools.

![image](https://github.com/jbakeacake/EdgeFinder_Tool/assets/34492737/a6fc2db4-7d6a-404b-b302-48d9c66ac3a7)

Next, we'll want to select some edges to generate our spline along. Go ahead and select the ![image](https://github.com/jbakeacake/EdgeFinder_Tool/assets/34492737/6c9b15c9-fb18-478f-9597-ad34677f450b) Edge Tool and begin picking
out some edges. Probuilder makes things a bit easy with their ![image](https://github.com/jbakeacake/EdgeFinder_Tool/assets/34492737/24677917-5f04-4d9a-be78-d4041384c13c) tool.

With those edges selected, go ahead and select Probuilder's ![image](https://github.com/jbakeacake/EdgeFinder_Tool/assets/34492737/62bc8f36-5400-48f5-95ee-ff2d9584fb69) vertex color picker, and then apply any color you'd like. For now I'm going to choose red.

> ℹ️ It's important to note that this tool relies on vertex colors to determine the unique edges for a spline. If you're using a shader that depends on those values, you may need to look into modifying the `EdgeFinder` algorithm to support
> edge finding via a different color map (e.g. a texture).

If you want to double check that the vertex color was applied correctly, you can set the mesh's material to Unity's `UnlitVertexColor.mat`. Adding that material, your object should look something like this:

![image](https://github.com/jbakeacake/EdgeFinder_Tool/assets/34492737/12ec0581-120c-46c4-b388-283820d9e42e)

#### Step 2. Add EdgeFinder.cs and Generate a Spline

Once you've added the tool to your project, you should be able to add the `EdgeFinder` component to any GameObject from the inspector panel.\
![image](https://github.com/jbakeacake/EdgeFinder_Tool/assets/34492737/e8d8f4f0-0e35-4aa3-a39c-e820815dfa13)

After adding the `EdgeFinder` script to one of your GameObjects, you should see a few options related to generating a spline.

![image](https://github.com/jbakeacake/EdgeFinder_Tool/assets/34492737/845c590d-b268-4958-902c-a3992e8190c3)

For now, let's ramp up the **Ledge Collider Radius** value to **0.5** and generate our spline! Simply click the "Generate Spline" button and you should see some child objects populate in your object's hierarchy.

![image](https://github.com/jbakeacake/EdgeFinder_Tool/assets/34492737/3afd58a0-86ce-4501-8abe-45285c9f30fa)

The first object that gets generated is the unique color value of the edge loop we defined out in step 1. Inside that is our generated spline and it's mesh collider! Highlighting the **Spline** object will enable DreamTeck's spline editor and it'll 
give us a quick preview of the Mesh Collider that was generated.

![image](https://github.com/jbakeacake/EdgeFinder_Tool/assets/34492737/0a5d550d-22be-4be4-aa4b-0e34660261d9)

If everything goes smoothly, you'll have successfully generated a spline for your mesh's edges! Feel free to add more loops with their own color to test things out.

#### Step 3. Testing

I won't go too much depth into testing since DreamTeck covers most of the usage around their splines. If you just want to see an object ride along the edge of your generated Spline, add the [SplineFollower](https://dreamteck-splines.netlify.app/#/./pages/tracing_splines/tracing_splines?id=spline-follower) component to another GameObject, set the generated Spline as the follower's `SplineComputer`, click *Run* and see it go!

---

### Inspector Values & Definitions

#### Default Spline Type : DreamTeck.Spline.Type
The default DreamTeck spline type to apply to all generated splines. The current allowed values are:
- HERMITE
- BEZIER
- B-SPLINE
- LINEAR

You can read more about DreamTeck's spline types [here](https://dreamteck-splines.netlify.app/#/./pages/spline_computer_settings/spline_computer_settings?id=type).

---

#### Spline Layer : string
This is the Unity layer that the spline and it's associated mesh collider will live on. It's recommended that you add this to a specific layer for querying (e.g. "Spline", "ClimbableLedge", "EdgeSpline", etc.).

⭐For some reason, Unity 2022.3 does **not** like using LayerMasks during in-editor operations. Leaving this as a string until a solution is found.

---

#### Ledge Collider Radius : float
This is a float value that will determine the height of the ledge mesh collider that gets generated along with the spline. This mesh collider is built in the **downwards** direction along the normal of the spline's control point.

⭐Mesh Collider generation now uses DreamTeck's `TubeGenerator` to create a the ledge mesh.

---

#### Draw All Colored Vertices : bool
Checking this value to true will draw all colored vertices when generating the spline.

---

#### Draw Contiguous Points : bool
Checking this value to true will draw all contiguous points found along an edge loop.

---

#### Draw Ledge Mesh : bool
Checking this value to true will draw the corresponding Mesh for the Mesh Collider.

---

#### Debug Material : Material
This is the material to be used when drawing the vertices mentioned above.

---

#### Generate Spline
This will trigger the procedural methods to create a spline and its adjoining mesh collider. **Make sure you've marked all the unique edge loops with a unique vertex color before proceeding with this step!**

---

#### Draw Spline Tangents
This is a debug tool that iterate through each control point on the generated spline and draw an arrow describing the direction of each sample point. If no spline is present, one will be generated using the same methods as the **Generate Spline** method.

---

#### Reset
:warning: Destroys all generated objects during spline generation! :warning: \
This effectively resets the object for spline generation; it will destroy only generated objects from spline generation.
