#I "../packages/"
#load "WebSharper.Warp/tools/reference-nover.fsx"

open WebSharper
open WebSharper.UI.Next
open WebSharper.UI.Next.Html
open WebSharper.UI.Next.Client
open WebSharper.JavaScript
open WebSharper.Sitelets

[<AutoOpen; JavaScript>]
module Utils =
    module Dom =
        module Element =
            let getId (x: Dom.Element) =
                x.GetAttribute "id"

[<JavaScript>]
module Sortable =
    
    [<Direct "Sortable.create($el, $options)">]
    let private sortableJS (el: Dom.Element) options = X<unit>

    type Sortable = {
        [<Name "group">]
        Group: Group
        
        [<Name "sort">]
        Sort: bool
        
        [<Name "animation">]
        Animation: int

        [<Name "onStart">]
        OnStart: SortableEvent-> unit

        [<Name "onEnd">]
        OnEnd: SortableEvent -> unit
        
        [<Name "onAdd">]
        OnAdd: SortableEvent-> unit
        
        [<Name "onUpdate">]
        OnUpdate: SortableEvent -> unit

        [<Name "onSort">]
        OnSort: SortableEvent-> unit

        [<Name "onRemove">]
        OnRemove: SortableEvent -> unit

        [<Name "onFilter">]
        OnFilter: SortableEvent -> unit
        
        [<Name "onMove">]
        OnMove: SortableEvent -> unit
    }
        with
            static member Default =
                { Group = 
                    { Name = ""
                      Pull = Pull.Allow |> Pull.ConvertToJSOption
                      Put  = Put.Allow  |> Put.ConvertToJSOption }
                  Sort = true
                  Animation = 150
                  OnStart = Unchecked.defaultof<_>
                  OnEnd = Unchecked.defaultof<_>
                  OnAdd = Unchecked.defaultof<_>
                  OnUpdate = Unchecked.defaultof<_>
                  OnSort = Unchecked.defaultof<_>
                  OnFilter = Unchecked.defaultof<_>
                  OnRemove = Unchecked.defaultof<_>
                  OnMove = Unchecked.defaultof<_> }

            static member SetGroup group (x: Sortable) =
                { x with Group = group }

            static member AllowSort (x: Sortable) =
                { x with Sort = true }

            static member DisallowSort (x: Sortable) =
                { x with Sort = false }
            
            static member SetOnAdd onAdd (x: Sortable) =
                { x with OnAdd = onAdd }

            //Runs only when sorting and not during drag and drop
            static member SetOnSort onSort (x: Sortable) =
                { x with OnSort = (fun e -> if Dom.Element.getId e.From = Dom.Element.getId e.To then onSort e) }
                
            static member Create el (x: Sortable) =
                sortableJS el x

    and Group = {
        [<Name "name">]
        Name: string
        
        [<Name "pull">]
        Pull: string
        
        [<Name "put">]
        Put: string
    }
        with
            static member Create name pull put =
                { Name = name
                  Pull = pull |> Pull.ConvertToJSOption 
                  Put  = put  |> Put.ConvertToJSOption }
    
    and Pull =
    | Allow
    | Disallow
    | Clone
        with
            static member ConvertToJSOption =
                function
                | Allow    -> "true"
                | Disallow -> "false"
                | Clone    -> "clone"

    and Put =
    | Allow
    | Disallow
    | AllowList of string list
        with
            static member ConvertToJSOption =
                function
                | Put.Allow          -> "true" 
                | Put.Disallow       -> "false" 
                | Put.AllowList list ->  list |> (String.concat "," >> sprintf "[%s]")

    and SortableEvent = {
        [<Name "item">]
        Item: Dom.Element
        
        [<Name "from">]
        From: Dom.Element
        
        [<Name "to">]
        To: Dom.Element

        [<Name "newIndex">]
        NewIndex: int
        
        [<Name "oldIndex">]
        OldIndex: int
    }

[<JavaScript>]
module Client =
    open Sortable
    
    let panel title body =
        divAttr [ attr.``class`` "panel panel-default" ]
                [ divAttr [ attr.``class`` "panel-heading" ] [ text title ]
                  divAttr [ attr.``class`` "panel-body" ] [ body] ]

    let main() =
        divAttr [ attr.``class`` "row" ]
                [ divAttr [ attr.``class`` "col-sm-4" ]
                          [ panel
                                "Workspace: droppable from ListA and ListB"
                                (divAttr [ attr.style "min-height:100px;"
                                           on.afterRender(fun el -> 
                                             Sortable.Default
                                             |> Sortable.SetGroup (Group.Create "workspace" Pull.Allow <| Put.AllowList [ "listA"; "listB" ])
                                             |> Sortable.Create el) ]
                                          []) ]
                  
                  divAttr [ attr.``class`` "col-sm-4" ]
                          [ panel
                                "ListA: draggable and sortable"
                                (divAttr [ on.afterRender(fun el -> 
                                             Sortable.Default
                                             |> Sortable.AllowSort
                                             |> Sortable.SetGroup (Group.Create "listA" Pull.Allow Put.Disallow)
                                             |> Sortable.Create el) ]
                                         [ div [ text "Aa" ]
                                           div [ text "Bb" ]
                                           div [ text "Cc" ]
                                           div [ text "Dd" ]
                                           div [ text "Ee" ] ]) ]
                  
                  divAttr [ attr.``class`` "col-sm-4" ]
                          [ panel
                                "ListB: draggable and cloned"
                                (ulAttr [ on.afterRender(fun el -> 
                                              Sortable.Default
                                              |> Sortable.DisallowSort
                                              |> Sortable.SetGroup (Group.Create "listB" Pull.Clone Put.Disallow)
                                              |> Sortable.Create el) ]
                                         [ li [ text "11" ]
                                           li [ text "22" ]
                                           li [ text "33" ]
                                           li [ text "44" ]
                                           li [ text "55" ] ]) ] ]
                                    
       


module Server =
    
    type Page = { Body: Doc list }

    let template =
        Content.Template<Page>(__SOURCE_DIRECTORY__ + "/index.html")
            .With("body", fun x -> x.Body)
    
    let site =
        Application.SinglePage (fun _ ->
            Content.WithTemplate template
                { Body = [ divAttr [ attr.style "padding:15px;" ] [ client <@ Client.main() @> ] ] })


do Warp.RunAndWaitForInput Server.site |> ignore