using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentraAI.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalOnboardingAnd18TracksSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 10,
                column: "OptionsJson",
                value: "[\"ada\", \"amazon redshift\", \"amazon web services (aws)\", \"angular\", \"angularjs\", \"ansible\", \"apt\", \"asp.net\", \"asp.net core\", \"assembly\", \"astro\", \"axum\", \"bash/shell (all shells)\", \"bigquery\", \"blazor\", \"bun\", \"c\", \"c#\", \"c++\", \"cargo\", \"cassandra\", \"chocolatey\", \"clickhouse\", \"cloud firestore\", \"cloudflare\", \"cobol\", \"cockroachdb\", \"composer\", \"cosmos db\", \"dart\", \"databricks sql\", \"datadog\", \"datomic\", \"delphi\", \"deno\", \"digital ocean\", \"django\", \"dynamodb\", \"express\", \"firebase\", \"gdscript\", \"gradle\", \"homebrew\", \"influxdb\", \"kotlin\", \"lua\", \"maven (build tool)\", \"microsoft sql server\", \"mysql\", \"new relic\", \"npm\", \"oracle\", \"php\", \"podman\", \"prolog\", \"railway\", \"ruby on rails\", \"splunk\", \"supabase\", \"terraform\", \"vercel\", \"docker\", \"elasticsearch\", \"f#\", \"firebase realtime database\", \"gleam\", \"groovy\", \"html/css\", \"java\", \"kubernetes\", \"make\", \"micropython\", \"drupal\", \"elixir\", \"fastapi\", \"flask\", \"duckdb\", \"erlang\", \"fastify\", \"fortran\", \"mojo\", \"go\", \"h2\", \"ibm cloud\", \"javascript\", \"laravel\", \"mariadb\", \"microsoft access\", \"mongodb\", \"webpack\", \"zig\", \"google cloud\", \"heroku\", \"ibm db2\", \"jquery\", \"lisp\", \"matlab\", \"microsoft azure\", \"msbuild\", \"neo4j\", \"next.js\", \"nuget\", \"pacman\", \"pip\", \"poetry\", \"prometheus\", \"react\", \"rust\", \"spring boot\", \"svelte\", \"typescript\", \"visual basic (.net)\", \"wordpress\", \"nestjs\", \"ninja\", \"nuxt.js\", \"perl\", \"pnpm\", \"postgresql\", \"python\", \"redis\", \"scala\", \"sql\", \"swift\", \"valkey\", \"vite\", \"netlify\", \"node.js\", \"ocaml\", \"phoenix\", \"pocketbase\", \"powershell\", \"r\", \"ruby\", \"snowflake\", \"sqlite\", \"symfony\", \"vba\", \"vue.js\", \"yandex cloud\", \"yarn\"]");

            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 11,
                column: "OptionsJson",
                value: "[\"ada\", \"amazon redshift\", \"amazon web services (aws)\", \"angular\", \"angularjs\", \"ansible\", \"apt\", \"asp.net\", \"asp.net core\", \"assembly\", \"astro\", \"axum\", \"bash/shell (all shells)\", \"bigquery\", \"blazor\", \"bun\", \"c\", \"c#\", \"c++\", \"cargo\", \"cassandra\", \"chocolatey\", \"clickhouse\", \"cloud firestore\", \"cloudflare\", \"cobol\", \"cockroachdb\", \"composer\", \"cosmos db\", \"dart\", \"databricks sql\", \"datadog\", \"datomic\", \"delphi\", \"deno\", \"digital ocean\", \"django\", \"dynamodb\", \"express\", \"firebase\", \"gdscript\", \"gradle\", \"homebrew\", \"influxdb\", \"kotlin\", \"lua\", \"maven (build tool)\", \"microsoft sql server\", \"mysql\", \"new relic\", \"npm\", \"oracle\", \"php\", \"podman\", \"prolog\", \"railway\", \"ruby on rails\", \"splunk\", \"supabase\", \"terraform\", \"vercel\", \"docker\", \"elasticsearch\", \"f#\", \"firebase realtime database\", \"gleam\", \"groovy\", \"html/css\", \"java\", \"kubernetes\", \"make\", \"micropython\", \"drupal\", \"elixir\", \"fastapi\", \"flask\", \"duckdb\", \"erlang\", \"fastify\", \"fortran\", \"mojo\", \"go\", \"h2\", \"ibm cloud\", \"javascript\", \"laravel\", \"mariadb\", \"microsoft access\", \"mongodb\", \"webpack\", \"zig\", \"google cloud\", \"heroku\", \"ibm db2\", \"jquery\", \"lisp\", \"matlab\", \"microsoft azure\", \"msbuild\", \"neo4j\", \"next.js\", \"nuget\", \"pacman\", \"pip\", \"poetry\", \"prometheus\", \"react\", \"rust\", \"spring boot\", \"svelte\", \"typescript\", \"visual basic (.net)\", \"wordpress\", \"nestjs\", \"ninja\", \"nuxt.js\", \"perl\", \"pnpm\", \"postgresql\", \"python\", \"redis\", \"scala\", \"sql\", \"swift\", \"valkey\", \"vite\", \"netlify\", \"node.js\", \"ocaml\", \"phoenix\", \"pocketbase\", \"powershell\", \"r\", \"ruby\", \"snowflake\", \"sqlite\", \"symfony\", \"vba\", \"vue.js\", \"yandex cloud\", \"yarn\"]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 10,
                column: "OptionsJson",
                value: "[\"c#\", \"sql\", \"javascript\", \"python\", \"java\", \"html/css\"]");

            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 11,
                column: "OptionsJson",
                value: "[\"asp.net core\", \"docker\", \"kubernetes\", \"react\", \"angular\", \"next.js\"]");
        }
    }
}
