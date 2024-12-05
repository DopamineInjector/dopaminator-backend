using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dopaminator.Models;
using Microsoft.EntityFrameworkCore;

namespace Dopaminator.Services
{
    public class MintableService
    {
        private readonly AppDbContext _context;
        private readonly string _imagePath;

        public MintableService(AppDbContext context, string imagePath)
        {
            _context = context;
            _imagePath = imagePath;
        }

        public async Task<Mintable?> Mint()
        {
            var mintable = await _context.Mintables.FirstOrDefaultAsync(m => !m.Minted);
            if (mintable == null) return null;

            mintable.Minted = true;
            await _context.SaveChangesAsync();

            return mintable;
        }

        public async Task Init()
        {
            if (await _context.Mintables.AnyAsync()) return;

            var files = Directory.GetFiles(_imagePath, "*.*", SearchOption.TopDirectoryOnly)
                                 .Where(f => f.EndsWith(".png"))
                                 .ToList();

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var imageBytes = await File.ReadAllBytesAsync(file);

                var mintable = new Mintable
                {
                    Name = name,
                    Image = imageBytes,
                    Minted = false
                };

                await _context.Mintables.AddAsync(mintable);
            }

            await _context.SaveChangesAsync();
        }
    }
}