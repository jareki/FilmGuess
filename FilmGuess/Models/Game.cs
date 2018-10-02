using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmGuess.Models
{
    class Game:IDisposable
    {
        BlockingCollection<GameRound> rounds;        
        public int lives;
        public int round_nomber;
        public int answers;
        public int score;
        public int count;
        int max_votecount;
        bool is_ending;

        public Game()
        {
            rounds = new BlockingCollection<GameRound>(10);
            round_nomber = score = answers = count = 0;
            lives = 3;
            if (App.is_imdb)
                max_votecount = 1671420;
            else
                max_votecount = 410700;
            is_ending = false;

            Task s = new Task(Add);
            s.Start();            
        }

        async void Add()
        {
            while (max_votecount >= 100 && !is_ending)
            {
                GameRound round = new GameRound();
                await round.GenerateRound(max_votecount);

                if (round.Films.Count == 4)
                {
                    rounds.Add(round);
                    count++;
                    if (App.is_imdb)
                        max_votecount = round.Films[round.right_answer].ratingIMDbVoteCount;
                    else
                        max_votecount = round.Films[round.right_answer].ratingVoteCount;
                }
                else
                {
                    max_votecount = 95 * max_votecount / 100;
                }
            }
            rounds.CompleteAdding();
        }

        public GameRound Get()
        {
            GameRound item;            

            while (!rounds.IsCompleted)
            {
                if (rounds.TryTake(out item))
                {
                    round_nomber++;
                    return item;
                }
            }
            return null;
        }

        public void Dispose()
        {
            is_ending = true;
        }
    }
}
