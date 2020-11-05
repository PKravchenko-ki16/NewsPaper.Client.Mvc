using Microsoft.AspNetCore.Builder;

namespace NewsPaper.Client.Mvc.Configure
{
    public class ConfigureEndpoints
    {
        public static void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "Default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
