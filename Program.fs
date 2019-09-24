open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive

[<CLIMutable>]
type Yo =
    {
        Word : string
    }

let simpleHandler =
    fun (next:HttpFunc) (ctx:HttpContext) ->
        task {
            let! yo = ctx.BindJsonAsync<Yo>()
            let yo2 = { yo with Word = "oy" }
            return! json yo2 next ctx
        }

let webApp =
    choose [
        route "/ping"   >=> text "pong"
        route "/yo"     >=> simpleHandler
        route "/"       >=> htmlFile "/pages/index.html" ]

let configureApp (app : IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    // Add Giraffe dependencies
    services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .Build()
        .Run()
    0
