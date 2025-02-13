using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManagement.Models;
using System.Security.Claims;

namespace PersonalFinanceManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="User")]
    public class TransactionController : ControllerBase
    {
        private readonly FinanceContext _context;

        public TransactionController(FinanceContext context)
        {
            _context = context;
        }

        // GET: api/transaction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
        {
            // Extract the user ID from the JWT
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Fetch transactions belonging only to the authenticated user
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            return Ok(transactions);
        }

        // GET: api/transaction/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            // Extract the user ID from the JWT
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Fetch a single transaction for the authenticated user
            var transaction = await _context.Transactions
                .Where(t => t.UserId == userId && t.Id == id)
                .FirstOrDefaultAsync();

            if (transaction == null)
                return NotFound();

            return Ok(transaction);
        }

        // POST: api/transaction
        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction(Transaction transaction)
        {
            // Extract the user ID from the JWT and assign it to the transaction
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            transaction.UserId = userId;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTransaction", new { id = transaction.Id }, transaction);
        }

        // PUT: api/transaction/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int id, Transaction transaction)
        {
            // Ensure the authenticated user is trying to update their own transaction
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (id != transaction.Id)
                return BadRequest();

            var existingTransaction = await _context.Transactions
                .Where(t => t.UserId == userId && t.Id == id)
                .FirstOrDefaultAsync();

            if (existingTransaction == null)
                return NotFound();

            // Update the transaction
            existingTransaction.Amount = transaction.Amount;
            existingTransaction.Category = transaction.Category;
            existingTransaction.Date = transaction.Date;
            existingTransaction.Description = transaction.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/transaction/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            // Ensure the authenticated user is deleting their own transaction
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var transaction = await _context.Transactions
                .Where(t => t.UserId == userId && t.Id == id)
                .FirstOrDefaultAsync();

            if (transaction == null)
                return NotFound();

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/transaction/summary
        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetSummary()
        {
            // Extract the user ID from the JWT
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Calculate summary data for the authenticated user
            var totalIncome = await _context.Transactions
                .Where(t => t.UserId == userId && t.Amount > 0)
                .SumAsync(t => t.Amount);

            var totalExpense = await _context.Transactions
                .Where(t => t.UserId == userId && t.Amount < 0)
                .SumAsync(t => t.Amount);

            return new
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = totalIncome + totalExpense
            };
        }
        // GET: api/transaction/date-range
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsInDateRange(DateTime startDate, DateTime endDate)
        {
            // Extract the user ID from the JWT
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Fetch transactions for the authenticated user within the specified date range
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound(new { message = "No transactions found in the specified date range." });
            }

            return Ok(transactions);
        }

        // GET: api/transaction/category-summary
        [HttpGet("category-summary")]
        public async Task<ActionResult<IEnumerable<object>>> GetCategorySummary()
        {
            // Extract the user ID from the JWT
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Group transactions by category and calculate the total amount for each category
            var categorySummary = await _context.Transactions
                .Where(t => t.UserId == userId)
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            if (!categorySummary.Any())
            {
                return NotFound(new { message = "No category summary found for this user." });
            }

            return Ok(categorySummary);
        }
             
    }
}
