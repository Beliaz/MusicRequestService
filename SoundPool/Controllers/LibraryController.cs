using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoundPool.Data;
using SoundPool.Data.EFCore;

namespace SoundPool.Controllers
{
    [Route("soundpool/api/[controller]")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly LibraryContext _dbContext;

        public LibraryController(LibraryContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("Songs")]
        public ActionResult<IEnumerable<string>> GetSongs()
        {
            return Ok(_dbContext
                .Songs
                .Include(s => s.Artists));
        }

        [HttpGet("Songs/{id}")]
        public ActionResult<IEnumerable<string>> GetSong(string id)
        {
            return Ok(_dbContext
                .Songs.Where(s => s.Id.Equals(id))
                .Include(s => s.Artists)
                .First());
        }

        [HttpGet("Artists")]
        public ActionResult<IEnumerable<string>> GetArtists()
        {
            return Ok(_dbContext.Artists);
        }

        [HttpGet("Artists/{id}")]
        public ActionResult<IEnumerable<string>> GetArtist(string name)
        {
            return Ok(_dbContext.Artists
                .First(s => s.Name.Equals(name)));
        }

        [HttpPost("Songs")]
        public ActionResult PostSong(CreateSongRequest request)
        {
            var artist = _dbContext.Artists.FirstOrDefault(a => a.Name.Equals(request.Artists.First()));
            var exisitingSongs = _dbContext.Songs.Where(s => request.Title.Equals(s.Title));

            if (exisitingSongs.Any() && artist != null)
            {
                var song = exisitingSongs
                    .Include(s => s.Artists)
                    .FirstOrDefault(s => s.Artists.All(a => a.Artist.Equals(artist)));

                if (song != null)
                    return Ok(song.Id);
            }

            if (artist is null)
            {
                foreach (var artistName in request.Artists
                    .Where(ra => !_dbContext.Artists
                        .Any(a => a.Name.Equals(ra))))
                {
                    _dbContext.Artists.Add(new Artist {Name = artistName});
                }
            }

            var newSong = new Song
            {
                Title = request.Title,
                Artists = _dbContext.Artists.Local
                    .Select(a => new SongArtist {Artist = a})
                    .ToList()
            };

            _dbContext.Songs.Add(newSong);
            _dbContext.SaveChanges();

            return Ok(newSong.Id);
        }
    }
}