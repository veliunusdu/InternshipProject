var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped(sp =>
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    };
    return new HttpClient(handler)
    {
        BaseAddress = new Uri("http://localhost:5000/")
    };
});
builder.Services.AddScoped<Project2.ViewModels.NoteListViewModel>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();