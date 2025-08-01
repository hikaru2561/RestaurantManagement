﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using RestaurantManagement.Helpers;
using System.Security.Claims;

namespace RestaurantManagement.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Lấy CustomerId từ Claim thay vì Username
        private int GetCustomerId()
        {
            var customerIdStr = User.FindFirst("CustomerId")?.Value;
            return int.TryParse(customerIdStr, out int id) ? id : 0;
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.DingningTables = new SelectList(_context.DingningTables.ToList(), "DingningTableId", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult Create(Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    reservation.CustomerId = GetCustomerId();
                    _context.Reservations.Add(reservation);
                    _context.SaveChanges();

                    var customer = _context.Customers.Find(reservation.CustomerId);
                    NotificationHelper.AddNotification(
                        _context,
                        "Đặt bàn mới",
                        $"Khách hàng {customer?.Name} đã đặt bàn {reservation.DingningTableId} vào {reservation.ReservationTime:dd/MM/yyyy HH:mm}.",
                        "admin",
                        "Admin"
                    );

                    TempData["Success"] = "Đặt bàn thành công!";
                    return RedirectToAction("History");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Lỗi khi đặt bàn: {ex.Message}";
                }
            }
            else
            {
                TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            }

            ViewBag.DingningTables = new SelectList(_context.DingningTables.ToList(), "DingningTableId", "Name", reservation.DingningTableId);
            return View(reservation);
        }

        [HttpGet]
        public IActionResult History()
        {
            int customerId = GetCustomerId();
            var list = _context.Reservations
                .Include(r => r.DingningTable)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.ReservationTime)
                .ToList();

            return View(list);
        }
        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var reservation = _context.Reservations.FirstOrDefault(r => r.ReservationId == id && r.Status == ReservationStatus.Pending);
            if (reservation == null)
            {
                TempData["Error"] = "Không thể huỷ lịch đặt này.";
                return RedirectToAction("History");
            }

            reservation.Status = ReservationStatus.Canceled;
            _context.SaveChanges();

            TempData["Success"] = "Huỷ lịch đặt bàn thành công.";
            return RedirectToAction("History");
        }

    }
}
