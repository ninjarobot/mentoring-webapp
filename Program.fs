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
    member this.SpeakAndRespond () =
        Console.WriteLine this.Word
        let newYo = { this with Word = "hello" }
        newYo

let speakAndRespond (yo:Yo) =
    Console.WriteLine yo.Word
    let newYo = { yo with Word = "hello" }
    newYo

let speak (yo:Yo) =
    Console.WriteLine yo.Word
    yo

let respond (yo:Yo) =
    let newYo = { yo with Word = "hello" }
    newYo

let speakAndRespond2 = speak >> respond


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

// Exception - you didn't expect them (network failure, disk problem, authentication issue)
// C#, Base Class Libraries (BCL) - try/catch/finally

// Error - things went wrong that you planned for

let rng = Random ()
let doSomethingThatMightFail () =
    if rng.Next () % 2 = 0 then
        failwith "random failure"
    else
        "it worked fine"

let trySomethingThatMightFail () =
    try
        doSomethingThatMightFail ()
        |> Result.Ok
    with
    | ex ->
        System.Console.Error.WriteLine "it failed"
        Result.Error "it failed"

let showErrorIfHappened result =
    match result with
    | Ok success -> Ok success
    | Error err ->
        eprintfn "An error happened: %s" err
        Error err

let showErrorIfHappenedConcise = Result.mapError (fun err ->
        eprintfn "An error happened: %s" err
        err
    )

let printResultOfSomething result =
    match result with
    | Ok success -> printfn "Result: %s" success
    | Error failure -> eprintfn "Failed: %s" failure
    
let doAndPrint = trySomethingThatMightFail >> showErrorIfHappenedConcise >> printResultOfSomething

[<EntryPoint>]
let main _ =
    while true do
        doAndPrint () 
        Console.ReadLine () |> ignore
    (*
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .Build()
        .Run()
    *)
    0
