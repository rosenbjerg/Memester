using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Memester.Core;
using Memester.Database;
using Memester.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Memester.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [SessionAuthentication]
    public class ProfileController : ControllerBase
    {
        private readonly OperationContext _operationContext;
        private readonly DatabaseContext _databaseContext;

        public ProfileController(OperationContext operationContext, DatabaseContext databaseContext)
        {
            _operationContext = operationContext;
            _databaseContext = databaseContext;
        }

        [HttpPost("vote")]
        public async Task Vote([FromRoute, Required]long memeId)
        {
            var meme = await _databaseContext.Memes.SingleAsync(m => m.Id == memeId);
        }
    }

    // public interface IEvent<TReturn>
    // {
    // }
    //
    // public interface IEventHandler<TEvent, TReturn>
    //     where TEvent : IEvent<TReturn>
    // {
    //     Task<TReturn> Handle(TEvent @event);
    // }
    // public interface IEventHandler<TEvent>
    //     where TEvent : IEvent
    // {
    //     Task<TReturn> Handle(TEvent @event);
    // }
    //
    // public class LikeEventHandler : IEventHandler<LikeEvent>
    // {
    //     private readonly DatabaseContext _databaseContext;
    //
    //     public LikeEventHandler(DatabaseContext databaseContext)
    //     {
    //         _databaseContext = databaseContext;
    //     }
    //     
    //     public async Task Handle(LikeEvent @event)
    //     {
    //         if (!@event.Like)
    //         {
    //             var like = _databaseContext.Likes.SingleAsync(l => l.Meme.Id == @event.MemeId && l.User.Id == @event.UserId);
    //             _databaseContext.Remove(like);
    //             await _databaseContext.SaveChangesAsync();
    //         }
    //         else
    //         {
    //             var meme = await _databaseContext.Memes.SingleAsync(m => m.Id == @event.MemeId);
    //             var user = await _databaseContext.Users.SingleAsync(m => m.Id == @event.UserId);
    //
    //             var like = new Like
    //             {
    //                 User = user,
    //                 Meme = meme
    //             };
    //             _databaseContext.Add(like);
    //             await _databaseContext.SaveChangesAsync();
    //         }
    //     }
    // }
    //
    // public class EventDispatcher
    // {
    //     private readonly IServiceProvider _serviceProvider;
    //
    //     public EventDispatcher(IServiceProvider serviceProvider)
    //     {
    //         _serviceProvider = serviceProvider;
    //     }
    //     
    //     Task Dispatch<TEvent>(TEvent @event)
    //     {
    //         var handlerType = typeof(IEventHandler<>).MakeGenericType(typeof(TEvent));
    //         var handler = (IEventHandler)_serviceProvider.GetRequiredService(handlerType);
    //         handler.
    //     }
    // }
    // public class LikeEvent : IEvent<bool>
    // {
    //     public long MemeId { get; set; }
    //     public long UserId { get; set; }
    //     public bool Like { get; set; }
    // }
}