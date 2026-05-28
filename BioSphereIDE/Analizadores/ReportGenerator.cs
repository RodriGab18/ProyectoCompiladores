// ============================================================================
//  ReportGenerator.cs
//  Genera el informe astrobiológico de simulación ASTRA.
//
//  Recopila las salidas de mostrar() y reporte(), calcula el Índice de
//  Similitud Terrestre (ESI), genera firmas espectrales y biofirmas
//  planetarias, y produce un informe formateado listo para revisión.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Text;
using BioSphereIDE.Core;

namespace BioSphereIDE.Analizadores
{
    public sealed class ReportGenerator
    {
        // ── Referencia terrestre ──────────────────────────────────────────────
        private const double EarthRadius   = 6371.0; // km
        private const double EarthTempK    = 288.15; // K  (15 °C)
        private const double EarthPressure = 1.0;    // atm

        // ── Pesos ESI (Schulze-Makuch et al. 2011) ────────────────────────────
        private const double WRadius   = 0.57;
        private const double WMass     = 1.07;
        private const double WTemp     = 5.58;
        private const double WPressure = 0.70;
        private const double TotalW    = WRadius + WMass + WTemp + WPressure; // 7.92

        // ── Punto de entrada ─────────────────────────────────────────────────
        public string GenerarInforme(
            PlanetaParametros p,
            InterpreterResult resultado,
            bool compilacionSinErrores)
        {
            double esi      = CalcularESI(p);
            string estado   = EstadoPlaneta(p, esi);
            string espectro = EspectroOptico(p);
            string biomasa  = EstadoBiomasa(p);
            var    biof     = Biofirmas(p);

            var sb = new StringBuilder();
            sb.AppendLine("==========================================");
            sb.AppendLine("          INFORME DE SIMULACIÓN ASTRA");
            sb.AppendLine("==========================================");

            // Salidas capturadas por mostrar() y reporte()
            if (resultado.SalidasMostrar.Count > 0 || resultado.SalidasReporte.Count > 0)
            {
                sb.AppendLine("  [Salidas de Simulación]");
                foreach (var s in resultado.SalidasMostrar)
                    sb.AppendLine($"  mostrar  →  {s}");
                foreach (var s in resultado.SalidasReporte)
                    sb.AppendLine($"  reporte  →  {s}");
                sb.AppendLine("------------------------------------------");
            }

            // Métricas principales
            sb.AppendLine($"* Planeta: {estado}");
            sb.AppendLine($"* Índice de Similitud Terrestre (ESI): {esi:F2}");
            sb.AppendLine($"* Temperatura Superficial Promedio: {p.planet_temp:F1} °C");
            sb.AppendLine($"* Estado de la Biomasa: {biomasa}");
            sb.AppendLine($"* Espectro de Biofirma: {espectro}");

            // Biofirmas detectadas
            if (biof.Count > 0)
            {
                sb.AppendLine("------------------------------------------");
                sb.AppendLine("  [Biofirmas Detectadas]");
                foreach (var bf in biof)
                    sb.AppendLine($"  ✓ {bf}");
            }

            // Parámetros físicos detallados
            sb.AppendLine("------------------------------------------");
            sb.AppendLine($"  Radio planetario : {p.radius:F0} km");
            sb.AppendLine($"  Masa planetaria  : {p.planet_mass:F2} M⊕");
            sb.AppendLine($"  Dist. estrella   : {p.star_distance_au:F2} UA");
            sb.AppendLine($"  Presión atm.     : {p.atm_pressure:F2} atm");
            sb.AppendLine($"  CO₂ atmosférico  : {p.atm_co2 * 1_000_000:F0} ppm");
            sb.AppendLine($"  O₂/N₂ atm.       : {p.atm_o2_n2 * 100:F1} %");
            sb.AppendLine($"  Cobertura de agua: {p.planet_water * 100:F1} %");
            sb.AppendLine($"  Cobertura vegetal: {p.planet_vegetation * 100:F1} %");
            sb.AppendLine($"  Act. tectónica   : {p.tectonic_activity * 100:F1} %");
            sb.AppendLine("==========================================");

            // Status final
            if (compilacionSinErrores && resultado.Exitosa)
                sb.Append("STATUS: COMPILACIÓN Y SIMULACIÓN EXITOSA ✓");
            else if (!compilacionSinErrores)
                sb.Append("STATUS: COMPILACIÓN CON ERRORES ✗");
            else
                sb.Append($"STATUS: ERROR EN EJECUCIÓN — {resultado.ErrorMensaje}");

            return sb.ToString();
        }

        // ── ESI: producto de factores individuales ────────────────────────────
        private static double CalcularESI(PlanetaParametros p)
        {
            double tempK = p.planet_temp + 273.15;

            return EsiFactor(p.radius    / EarthRadius,        1.0, WRadius)
                 * EsiFactor(p.planet_mass,                    1.0, WMass)
                 * EsiFactor(tempK       / EarthTempK,         1.0, WTemp)
                 * EsiFactor(p.atm_pressure / EarthPressure,   1.0, WPressure);
        }

        // ESI_i = (1 - |x - x_ref| / (x + x_ref))^(w / TotalW)
        private static double EsiFactor(double x, double xRef, double w)
        {
            double sum = x + xRef;
            if (sum <= 0) return 0;
            double b = 1.0 - Math.Abs(x - xRef) / sum;
            return Math.Pow(Math.Max(b, 0.0), w / TotalW);
        }

        // ── Habitabilidad ─────────────────────────────────────────────────────
        private static string EstadoPlaneta(PlanetaParametros p, double esi)
        {
            bool tempOk    = p.planet_temp is >= -20 and <= 60;
            bool aguaOk    = p.planet_water > 0.05f;
            bool presionOk = p.atm_pressure is > 0.1f and < 5.0f;
            bool estable   = tempOk && aguaOk && presionOk;

            string nivel = esi switch
            {
                >= 0.80 => "Habitable",
                >= 0.60 => "Potencialmente Habitable",
                >= 0.40 => "Marginalmente Habitable",
                _       => "No Habitable"
            };
            return $"{nivel} ({(estable ? "Estable" : "Inestable")})";
        }

        // ── Espectro óptico dominante ─────────────────────────────────────────
        private static string EspectroOptico(PlanetaParametros p)
        {
            if (p.atm_o2_n2 > 0.8f && p.planet_water > 0.4f && p.planet_vegetation > 0.4f)
                return "Translucido Celeste";
            if (p.planet_vegetation > 0.7f)
                return "Espectro Vegetal Verde";
            if (p.planet_water > 0.8f)
                return "Espectro Oceánico Profundo";
            if (p.atm_co2 > 0.01f)
                return "Firma Infrarroja CO₂";
            if (p.atm_methane > 0.001f)
                return "Firma Metano Biogénico";
            if (p.atm_o2_n2 > 0.5f)
                return "Espectro Celeste Tenue";
            return "Espectro Neutro / Árido";
        }

        // ── Estado de la biomasa ──────────────────────────────────────────────
        private static string EstadoBiomasa(PlanetaParametros p)
        {
            bool tempOk = p.planet_temp is > -10 and < 50;
            bool aguaOk = p.planet_water > 0.3f;
            bool vegOk  = p.planet_vegetation > 0.5f;

            return (tempOk, aguaOk, vegOk) switch
            {
                (true, true,  true)  => "Favorable",
                (true, true,  false) => "Moderado",
                (true, false, _)     => "Limitado",
                _                   => "Desfavorable"
            };
        }

        // ── Biofirmas detectadas ──────────────────────────────────────────────
        private static List<string> Biofirmas(PlanetaParametros p)
        {
            var lista = new List<string>();
            if (p.atm_o2_n2 > 0.3f)        lista.Add("Oxígeno Atmosférico (O₂)");
            if (p.planet_water > 0.1f)      lista.Add("Agua Líquida (H₂O)");
            if (p.planet_vegetation > 0.2f) lista.Add("Red Edge Vegetal (Clorofila)");
            if (p.atm_methane > 0.0001f)    lista.Add("Metano Biogénico (CH₄)");
            if (p.tectonic_activity > 0.2f) lista.Add("Actividad Tectónica");
            return lista;
        }
    }
}
