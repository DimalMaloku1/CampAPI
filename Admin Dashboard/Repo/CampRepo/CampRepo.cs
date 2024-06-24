using Core.Context;
using Microsoft.EntityFrameworkCore;

namespace Admin_Dashboard.Repo.CampRepo
{
    public class CampRepo(CampDbContext context) : ICampRepo
    {
        private readonly CampDbContext _context = context;

        public async Task DeleteCampChildAsync(int id)
        {
            var camps = await _context.ChildCamps.Where(c => c.CampId == id).ToListAsync();
            foreach (var camp in camps)
            {
                _context.ChildCamps.Remove(camp);
            }
            await _context.SaveChangesAsync();
        }
    }
}
