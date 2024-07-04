using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Movies.API.Controllers;
using Movies.Data.Models;
using Movies.Data.Interfaces;
using Movies.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;

namespace Movies.Tests
{
    public class MoviesControllerTest
    {
        private readonly MoviesContext _context;
        private readonly MovieRepositroy _repository;
        private readonly MoviesController _controller;

        public MoviesControllerTest()
        {
            _context = new MoviesContext();
            _repository = new MovieRepositroy(_context);
            _controller = new MoviesController(_repository);
        }

        [Fact]
        public void GetAllMovies_ReturnSuccessIfCorrectCount()
        {
            var result = _controller.GetMovies();

            Assert.IsType<ActionResult<IEnumerable<Movie>>>(result.Result);

            var list= result.Result as ActionResult<IEnumerable<Movie>>;

            Assert.IsType<OkObjectResult>(list.Result);

            var okResult = list.Result as OkObjectResult;

            Assert.IsType<List<Movie>>(okResult.Value);

            var movies = okResult.Value as List<Movie>;

            Assert.Equal(7, movies.Count);
        }

        [Fact]
        public void GetAllMovies_ReturnSuccessIfWrongCount()
        {
            var result = _controller.GetMovies();

            Assert.IsType<ActionResult<IEnumerable<Movie>>>(result.Result);

            var list = result.Result as ActionResult<IEnumerable<Movie>>;

            Assert.IsType<OkObjectResult>(list.Result);

            var okResult = list.Result as OkObjectResult;

            Assert.IsType<List<Movie>>(okResult.Value);

            var movies = okResult.Value as List<Movie>;

            Assert.NotEqual(5, movies.Count);
        }

        [Theory]
        [InlineData(4,"300")]
        [InlineData(5, "Dictator")]
        public void GetMovieById_ReturnOkObjectResult(int id,string title)
        {
            var okResult=_controller.GetMovie(id);
            
            Assert.IsType<ActionResult<Movie>>(okResult.Result);
            
            Assert.IsType<OkObjectResult>(okResult.Result.Result);

            Assert.IsType<Movie>((okResult.Result.Result as OkObjectResult).Value);

            var movie = (okResult.Result.Result as OkObjectResult).Value as Movie;
            Assert.Equal(id, movie.Id);
            Assert.Equal(title, movie.Title);
        }

        [Theory]
        [InlineData(67)]
        [InlineData(1)]
        public void GetMovieById_ReturnNotFound(int id)
        {
            var notFoundResult = _controller.GetMovie(id);

            Assert.IsType<ActionResult<Movie>>(notFoundResult.Result);
            Assert.IsType<NotFoundResult>(notFoundResult.Result.Result);
        }

        [Fact]
        public void Add_ValidObjectPassed_ReturnsCreatedResponse()
        {
            Movie newMovie = new Movie()
            {
                Id = 0,
                Title = "Alien",
                Genre = "Horror",
                Description = "A horror movie",
                ReleaseYear = "1979",
            };

            var createdResponse = _controller.PostMovie(newMovie);
            Assert.IsType<ActionResult<Movie>>(createdResponse.Result);
            Assert.IsType<CreatedAtActionResult>(createdResponse.Result.Result);
        }

        [Fact]
        public void Update_LastInsertedObject_ReturnOkResponse()
        {
            int id = _context.Movies.Max(m => m.Id);
            Movie updatemovie = new Movie()
            {
                Id = id,
                Title = "Pipi duga čarapa",
                Genre = "dječiji",
                Description = "Pipi i prijatelji",
                ReleaseYear = "1972",
            };

            var okResult = _controller.PutMovie(id, updatemovie);

            Assert.IsType<OkObjectResult>(okResult.Result);
            Assert.IsType<Movie>((okResult.Result as OkObjectResult).Value);
            var movie = (okResult.Result as OkObjectResult).Value as Movie;
            Assert.Equal(movie.Id, updatemovie.Id);
            Assert.Equal(movie.Title, updatemovie.Title);
            Assert.Equal(movie.Genre, updatemovie.Genre);
            Assert.Equal(movie.Description, updatemovie.Description);
            Assert.Equal(movie.ReleaseYear, updatemovie.ReleaseYear);
        }

        [Fact]
        public void Delete_LastInsertedObject_ReturnsOkResponse()
        {
            int id=_context.Movies.Max(m=>m.Id);
            var okResult=_controller.DeleteMovie(id);

            Assert.IsType<OkObjectResult>(okResult.Result);
        }

        [Fact]
        public void Add_InvalidObjectPassed_ReturnsBadRequest()
        {
            Movie invalidMovie = new Movie()
            {
                Title = "Titanic"
            };

            _controller.ModelState.AddModelError("Genre", "Required");
            _controller.ModelState.AddModelError("ReleaseYear", "Required");
            _controller.ModelState.AddModelError("Description", "Movie too long! Let him live!");

            var badResponse = _controller.PostMovie(invalidMovie);

            Assert.IsType<ActionResult<Movie>>(badResponse.Result);
            Assert.IsType<BadRequestObjectResult>(badResponse.Result.Result);
        }

        [Theory]
        [InlineData(666)]
        public void Remove_GetNonExistingMovieById_ReturnsNotFoundResult(int id)
        {
            var notFoundResult = _controller.DeleteMovie(id);
            Assert.IsType<NotFoundObjectResult>(notFoundResult.Result);
        }

        [Theory]
        [InlineData(4)]
        public void Update_IdMismatch_ReturnsBadRequest(int id) 
        {
            Movie movie=new Movie() { 
                Id= id + 1, 
                Title = "Pipi duga čarapa", 
                Genre = "dječiji", 
                Description = "Pipi i prijatelji", 
                ReleaseYear = "1972" 
            };

            var badResponse = _controller.PutMovie(id, movie);
            Assert.IsType<BadRequestObjectResult>(badResponse.Result); 
        }

        [Theory]
        [InlineData(666)]
        public void Update_GetNonExistingMovieById_ReturnsNotFoundResult(int id)
        { 
           Movie movie = new Movie()
            {
                Id = id,
                Title = "Pipi duga čarapa",
                Genre = "dječiji",
                Description = "Pipi i prijatelji",
                ReleaseYear = "1972"
            };

            var notFoundResult = _controller.PutMovie(id, movie);
            Assert.IsType<NotFoundObjectResult>(notFoundResult.Result);
        }
    }
}
