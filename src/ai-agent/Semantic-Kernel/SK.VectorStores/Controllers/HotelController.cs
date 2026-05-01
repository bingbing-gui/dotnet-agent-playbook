using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SK.VectorStores.Models;
using SK.VectorStores.Services;
using SK.VectorStores.ViewModes;
using System.Collections.Generic;

namespace SK.VectorStores.Controllers
{
    public class HotelController : Controller
    {
        private readonly IHotelService _svc;

        public HotelController(IHotelService svc) => _svc = svc;

        // 首页：显示搜索框（首次不查，或用默认TopK）
        public IActionResult Index() => View(new HotelSearchVm());

        // 向量搜索（文本 -> embedding -> 相似度TopK）
        public async Task<IActionResult> Search(HotelSearchVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View("Index", vm);

            var list = new List<(Hotel Record, double? Score)>();
            await foreach (var r in _svc.SearchByTextAsync(vm.Query ?? string.Empty, vm.TopK, ct))
            {
                list.Add(r);
            }
            vm.Results = list.Select(r => new HotelSearchResultVm
            {
                HotelId = r.Record.HotelId,
                HotelName = r.Record.HotelName,
                Description = r.Record.Description,
                Score = r.Score ?? 0
            }).ToList();
            return View("Index", vm);
        }

        // 详情
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var h = await _svc.GetAsync(id, ct);
            return h is null ? NotFound() : View(h);
        }
        [HttpGet]
        public IActionResult Create() => View(new Hotel());
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hotel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);
            await _svc.UpsertAsync(model, ct); // 会自动生成向量并保存
            return RedirectToAction(nameof(Details), new { id = model.HotelId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var h = await _svc.GetAsync(id, ct);
            return h is null ? NotFound() : View(h);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hotel model, CancellationToken ct)
        {
            if (id != model.HotelId) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            await _svc.UpsertAsync(model, ct); // 覆盖更新（含向量）
            return RedirectToAction(nameof(Details), new { id = model.HotelId });
        }

        // 删除
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return View("Index");
        }
    }

}
