#I "../packages/"
#load "WebSharper.Warp/tools/reference-nover.fsx"

open WebSharper
open WebSharper.JavaScript
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Html
open WebSharper.UI.Next.Client

[<JavaScript>]
module Client =
    let main() =
        Doc.Empty

module Server =
    let site =
        Application.SinglePage (fun _-> 
            Content.Page [ client <@ Client.main() @> ])


do Warp.RunAndWaitForInput Server.site |> ignore