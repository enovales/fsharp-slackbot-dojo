module Slack

type SlackRequest =
    {
        Token       : string option
        TeamId      : string option
        TeamDomain  : string option
        ChannelId   : string option
        ChannelName : string option
        UserId      : string option
        UserName    : string option
        Command     : string option
        Text        : string option
        ResponseUrl : string option
    }
    static member FromHttpContext (ctx : HttpContext) =
        let get key =
            match ctx.request.formData key with
            | Choice1Of2 x  -> Some(x)
            | _             -> None
        {
            Token       = get "token"
            TeamId      = get "team_id"
            TeamDomain  = get "team_domain"
            ChannelId   = get "channel_id"
            ChannelName = get "channel_name"
            UserId      = get "user_id"
            UserName    = get "user_name"
            Command     = get "command"
            Text        = get "text"
            ResponseUrl = get "response_url"
        }

type SlackResponseType = 
    | Ephemeral
    | InChannel
    with
        override this.ToString() = 
            match this with
            | Ephemeral -> "ephemeral"
            | InChannel -> "in_channel"

type SlackResponse = 
    {
        responseType: SlackResponseType
        text: string
    }
    with
        override this.ToString() = 
            "{ \"response_type\": \"" + this.responseType.ToString() + "\", \"text\": \"" + this.text + "\"}"
