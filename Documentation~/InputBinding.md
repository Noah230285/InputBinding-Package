# Input Binding

There are two primary parts to this package subset. The first is runtime input rebinding via UIToolkit,
the second is binding input paths to textures to use as representative icons. These bindings occur through
the new input system, so any project that wants to take use of these features will need to utilise that system
for their inputs.

Jump to
<br>
[**Input Remapping**](#input-remapping)\
[&emsp;> **1. Initial setup**](#initial-setup)\
[&emsp;> **2. UXML Assets**](#uxml-assets)\
[&emsp;&emsp;> **Create New Bindable UXML Assets**](#create-new-bindable-uxml-assets)\
[&emsp;&emsp;> **Using Existing UXML Assets**](#using-existing-uxml-assets)\
[**Input Icon Binding**](#input-icon-binding)

# Input Remapping
<br>

## 1. Initial Setup


There are three major components to setting up input rebinding.
1. The **UXML** assets that will be loaded into the scene and act as the runtime interface for input rebinding.
2. The **InputActionAsset** you will be using to store your input actions.
3. The **UXMLRemappingBinder** MonoBehaviour, which binds the UI to the actions and is essentially the brain of the rebinding.
<br>

If you would like to see an example on how to set up the input remapper along with these instructions, you can open a sample scene at;
<br>
Samples~\Input Remaping Example\InputRemapSample.unity
<br>


## 2. UXML Assets
This package comes with its own set of UXML and USS under \
/InputBinding/Assets/InputActionBinding/UIToolkit \
The most relevant of these being **InputRemapper**, which has all the elements to be used for a single action binding. There are also UXML elements for
the singular buttons used in **InputRemapper**, and 

If you want to create
your own UXML asset compatable with **UXMLRemappingBinder**, you can follow the instructions [here](#create-new-bindable-uxml-assets). Otherwise, if you want
to learn how to use and modify the included UXML assets provided, you can find that [here](#using-new-existing-uxml-assets)

<br>

### Create New Bindable UXML Assets
<br>

<div style="float: left">
<img style="float: left;padding-right: 20px" src="InputBinding/UXMLFlowGraph.png">

This accompanied graph shows the hierachy of Visual elements that is required for the **UXMLRemappingBinder** to find all of the elements for the UI 
to be properly bound.

To create a ***single input action rebinder** with the three rebindable binding paths, you will need a container
VisualElement that has a unique **name** (Shown as #EXAMPLENAME),
which is used by the [**UXMLRemappingBinder**](../CHANGELOG.md) to link its sub-buttons to a specific action. This element then needs to contain three
children elements with the names, **KeyboardPrimary**, **KeyboardSecondary**, **ControllerMapping**. Each of these need to have a **Button**
as their **first child**. Then finally, **all three** of these buttons need to have VisualElement children with the names **Icon** and **ButtonOverlay**.
These elements are used for the icon of the currently chosen input binding, and an overlay for when the button is disabled, respectively.

The final UXML asset used for the asset rebinding, it is recommended to include an overlay element to give feedback to the binding and allows
the display of relevent information, as well as stopping the player from interacting with anything else while the binding is taking place.
To do this, Implement a VisualElement with the name **BindingOverlay**, and give it two children **Labels**, called **TopLabel** and **BottomLabel**.

</div>

You will also need to create a new USS asset attached to these UXML assets with following USS
**classes** implemented with the following functionality:

1. **.hidden** \
    This is added/removed from **BindingOverlay** when a rebinding begins/ends, and should set the **display to none**

2. **.disabled** \
    This is added/removed from the rebinding **Buttons** when they have been disabled in the inspector,
    and should **display** the button's **ButtonOverlay** child

2. **.greyedOut** \
    This is added/removed from the rebinding **Buttons** when they can't/can be used based on the current control method,
    and should **display** the button's **ButtonOverlay** child
<br>

### Using Existing UXML Assets



<!--<img style="width: 600px" src="InputBinding/NameLinking.png">-->


<!--![](InputBinding/InspectorExample.png)
![](InputBinding/NameLinking.png)
-->

# Input Icon Binding
