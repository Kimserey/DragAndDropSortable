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
    let main() =
        Doc.Empty

module Server =
    let site =
        Application.SinglePage (fun _-> 
            Content.Page [ client <@ Client.main() @> ])


do Warp.RunAndWaitForInput Server.site |> ignore