#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Runtime
{
    public class SunRiseSetCalc
    {
        public double latitude;
        public double longitude;
        public int timeZone;

        public SunRiseSetCalc (double latitude, double longitude) {
            timeZone = TimeZoneInfo.Local.BaseUtcOffset.Hours;
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public DateSpan GetRiseTime (DateTime day) {
            double julianDate = CalcJD (day);
            double riseUTC = CalcSunRiseUtc (julianDate, latitude, longitude);
            DateSpan rise = new DateSpan (new Time (TimeSpan.FromMinutes (riseUTC + timeZone * 60)));
            if (TimeZoneInfo.Local.IsDaylightSavingTime (DateTime.Now))
                rise.AddMinutes (60);
            return rise;
        }

        public DateSpan GetSetTime (DateTime day) {
            double julianDate = CalcJD (day);
            double setUTC = CalcSunSetUtc (julianDate, latitude, longitude);
            DateSpan sSet = new DateSpan (new Time (TimeSpan.FromMinutes (setUTC + timeZone * 60)));
            if (TimeZoneInfo.Local.IsDaylightSavingTime (DateTime.Now))
                sSet.AddMinutes (60);
            return sSet;
        }

        protected double CalcJD (DateTime date) {
            return CalcJD (date.Year, date.Month, date.Day);
        }

        //***********************************************************************/
        //* All the following code derived from 
        //* http://www.esrl.noaa.gov/gmd/grad/solcalc/  
        //***********************************************************************/

        //***********************************************************************/
        //* Name: CalcJD	
        //* Type: Function	
        //* Purpose: Julian day from calendar day	
        //* Arguments:	
        //* year : 4 digit year	
        //* month: January = 1	
        //* day : 1 - 31	
        //* Return value:	
        //* The Julian day corresponding to the date	
        //* Note:	
        //* Number is returned for start of day. Fractional days should be	
        //* added later.	
        //***********************************************************************/
        protected double CalcJD (int year, int month, int day) {
            if (month <= 2) {
                year -= 1;
                month += 12;
            }
            double A = Math.Floor (year / 100.0);
            double B = 2 - A + Math.Floor (A / 4);

            double JD = Math.Floor (365.25 * (year + 4716)) + Math.Floor (30.6001 * (month + 1)) + day + B - 1524.5;
            return JD;
        }

        //***********************************************************************/
        //* Name: CalcTimeJulianCent	
        //* Type: Function	
        //* Purpose: convert Julian Day to centuries since J2000.0.	
        //* Arguments:	
        //* jd : the Julian Day to convert	
        //* Return value:	
        //* the T value corresponding to the Julian Day	
        //***********************************************************************/
        protected double CalcTimeJulianCent (double jd) {
            double T = (jd - 2451545.0) / 36525.0;
            return T;
        }

        //***********************************************************************/
        //* Name: CalcJDFromJulianCent	
        //* Type: Function	
        //* Purpose: convert centuries since J2000.0 to Julian Day.	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* the Julian Day corresponding to the t value	
        //***********************************************************************/
        protected double CalcJDFromJulianCent (double t) {
            double JD = t * 36525.0 + 2451545.0;
            return JD;
        }

        //***********************************************************************/
        //* Name: CalcGeomMeanLongSun	
        //* Type: Function	
        //* Purpose: calculate the Geometric Mean Longitude of the Sun	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* the Geometric Mean Longitude of the Sun in degrees	
        //***********************************************************************/
        protected double CalcGeomMeanLongSun (double t) {
            double L0 = 280.46646 + t * (36000.76983 + 0.0003032 * t);
            while (L0 > 360.0) {
                L0 -= 360.0;
            }
            while (L0 < 0.0) {
                L0 += 360.0;
            }
            return L0;	 // in degrees
        }

        //***********************************************************************/
        //* Name: CalcGeomMeanAnomalySun	
        //* Type: Function	
        //* Purpose: calculate the Geometric Mean Anomaly of the Sun	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* the Geometric Mean Anomaly of the Sun in degrees	
        //***********************************************************************/
        protected double CalcGeomMeanAnomalySun (double t) {
            double M = 357.52911 + t * (35999.05029 - 0.0001537 * t);
            return M;	 // in degrees
        }

        //***********************************************************************/
        //* Name: CalcEccentricityEarthOrbit	
        //* Type: Function	
        //* Purpose: calculate the eccentricity of earth's orbit	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* the unitless eccentricity	
        //***********************************************************************/
        protected double CalcEccentricityEarthOrbit (double t) {
            double e = 0.016708634 - t * (0.000042037 + 0.0000001267 * t);
            return e;	 // unitless
        }

        //***********************************************************************/
        //* Name: CalcSunEqOfCenter	
        //* Type: Function	
        //* Purpose: calculate the equation of center for the sun	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* in degrees	
        //***********************************************************************/
        protected double CalcSunEqOfCenter (double t) {
            double m = CalcGeomMeanAnomalySun (t);

            double mrad = m.ToRadians ();
            double sinm = Math.Sin (mrad);
            double sin2m = Math.Sin (mrad + mrad);
            double sin3m = Math.Sin (mrad + mrad + mrad);

            double C = sinm * (1.914602 - t * (0.004817 + 0.000014 * t)) + sin2m * (0.019993 - 0.000101 * t) + sin3m * 0.000289;
            return C;	 // in degrees
        }


        //***********************************************************************/
        //* Name: CalcSunTrueLong	
        //* Type: Function	
        //* Purpose: calculate the true longitude of the sun	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* sun's true longitude in degrees	
        //***********************************************************************/
        protected double CalcSunTrueLong (double t) {
            double l0 = CalcGeomMeanLongSun (t);
            double c = CalcSunEqOfCenter (t);

            double O = l0 + c;
            return O;	 // in degrees
        }

        //***********************************************************************/
        //* Name: CalcSunTrueAnomaly	
        //* Type: Function	
        //* Purpose: calculate the true anamoly of the sun	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* sun's true anamoly in degrees	
        //***********************************************************************/
        protected double CalcSunTrueAnomaly (double t) {
            double m = CalcGeomMeanAnomalySun (t);
            double c = CalcSunEqOfCenter (t);

            double v = m + c;
            return v;	 // in degrees
        }

        //***********************************************************************/
        //* Name: CalcSunRadVector	
        //* Type: Function	
        //* Purpose: calculate the distance to the sun in AU	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* sun radius vector in AUs	
        //***********************************************************************/
        protected double CalcSunRadVector (double t) {
            double v = CalcSunTrueAnomaly (t);
            double e = CalcEccentricityEarthOrbit (t);

            double R = (1.000001018 * (1 - e * e)) / (1 + e * Math.Cos (v.ToRadians ()));
            return R;	 // in AUs
        }

        //***********************************************************************/
        //* Name: CalcSunApparentLong	
        //* Type: Function	
        //* Purpose: calculate the apparent longitude of the sun	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* sun's apparent longitude in degrees	
        //***********************************************************************/
        protected double CalcSunApparentLong (double t) {
            double o = CalcSunTrueLong (t);

            double omega = 125.04 - 1934.136 * t;
            double lambda = o - 0.00569 - 0.00478 * Math.Sin (omega.ToRadians ());
            return lambda;	 // in degrees
        }

        //***********************************************************************/
        //* Name: CalcMeanObliquityOfEcliptic	
        //* Type: Function	
        //* Purpose: calculate the mean obliquity of the ecliptic	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* mean obliquity in degrees	
        //***********************************************************************/
        protected double CalcMeanObliquityOfEcliptic (double t) {
            double seconds = 21.448 - t * (46.8150 + t * (0.00059 - t * (0.001813)));
            double e0 = 23.0 + (26.0 + (seconds / 60.0)) / 60.0;
            return e0;	 // in degrees
        }

        //***********************************************************************/
        //* Name: CalcObliquityCorrection	
        //* Type: Function	
        //* Purpose: calculate the corrected obliquity of the ecliptic	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* corrected obliquity in degrees	
        //***********************************************************************/
        protected double CalcObliquityCorrection (double t) {
            double e0 = CalcMeanObliquityOfEcliptic (t);

            double omega = 125.04 - 1934.136 * t;
            double e = e0 + 0.00256 * Math.Cos (omega.ToRadians ());
            return e;	 // in degrees
        }

        //***********************************************************************/
        //* Name: CalcSunRtAscension	
        //* Type: Function	
        //* Purpose: calculate the right ascension of the sun	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* sun's right ascension in degrees	
        //***********************************************************************/
        protected double CalcSunRtAscension (double t) {
            double e = CalcObliquityCorrection (t);
            double lambda = CalcSunApparentLong (t);

            double tananum = (Math.Cos (e.ToRadians ()) * Math.Sin (lambda.ToRadians ()));
            double tanadenom = (Math.Cos (lambda.ToRadians ()));
            double alpha = Math.Atan2 (tananum, tanadenom).ToDegrees ();
            return alpha;	 // in degrees
        }

        //***********************************************************************/
        //* Name: CalcSunDeclination	
        //* Type: Function	
        //* Purpose: calculate the declination of the sun	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* sun's declination in degrees	
        //***********************************************************************/
        protected double CalcSunDeclination (double t) {
            double e = CalcObliquityCorrection (t);
            double lambda = CalcSunApparentLong (t);

            double sint = Math.Sin (e.ToRadians ()) * Math.Sin (lambda.ToRadians ());
            double theta = Math.Asin (sint).ToDegrees ();
            return theta;	 // in degrees
        }

        //***********************************************************************/
        //* Name: CalcEquationOfTime	
        //* Type: Function	
        //* Purpose: calculate the difference between true solar time and mean	
        //*	 solar time	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* Return value:	
        //* equation of time in minutes of time	
        //***********************************************************************/
        protected double CalcEquationOfTime (double t) {
            double epsilon = CalcObliquityCorrection (t);
            double l0 = CalcGeomMeanLongSun (t);
            double e = CalcEccentricityEarthOrbit (t);
            double m = CalcGeomMeanAnomalySun (t);

            double y = Math.Tan (epsilon.ToRadians () / 2.0);
            y *= y;

            double sin2l0 = Math.Sin (2.0 * l0.ToRadians ());
            double sinm = Math.Sin (m.ToRadians ());
            double cos2l0 = Math.Cos (2.0 * l0.ToRadians ());
            double sin4l0 = Math.Sin (4.0 * l0.ToRadians ());
            double sin2m = Math.Sin (2.0 * m.ToRadians ());

            double Etime = y * sin2l0 - 2.0 * e * sinm + 4.0 * e * y * sinm * cos2l0
            - 0.5 * y * y * sin4l0 - 1.25 * e * e * sin2m;

            return Etime.ToDegrees () * 4.0;	// in minutes of time
        }

        //***********************************************************************/
        //* Name: CalcHourAngleSunrise	
        //* Type: Function	
        //* Purpose: calculate the hour angle of the sun at sunrise for the	
        //*	 latitude	
        //* Arguments:	
        //* lat : latitude of observer in degrees	
        //*	solarDec : declination angle of sun in degrees	
        //* Return value:	
        //* hour angle of sunrise in radians	
        //***********************************************************************/
        protected double CalcHourAngleSunrise (double lat, double solarDec) {
            double latRad = lat.ToRadians ();
            double sdRad = solarDec.ToRadians ();

            double HA = (Math.Acos (Math.Cos ((90.833).ToRadians ()) / (Math.Cos (latRad) * Math.Cos (sdRad)) - Math.Tan (latRad) * Math.Tan (sdRad)));

            return HA;	 // in radians
        }

        //***********************************************************************/
        //* Name: CalcHourAngleSunset	
        //* Type: Function	
        //* Purpose: calculate the hour angle of the sun at sunset for the	
        //*	 latitude	
        //* Arguments:	
        //* lat : latitude of observer in degrees	
        //*	solarDec : declination angle of sun in degrees	
        //* Return value:	
        //* hour angle of sunset in radians	
        //***********************************************************************/
        protected double CalcHourAngleSunset (double lat, double solarDec) {
            double latRad = lat.ToRadians ();
            double sdRad = solarDec.ToRadians ();

            double HA = (Math.Acos (Math.Cos ((90.833).ToRadians ()) / (Math.Cos (latRad) * Math.Cos (sdRad)) - Math.Tan (latRad) * Math.Tan (sdRad)));

            return -HA;	 // in radians
        }


        //***********************************************************************/
        //* Name: CalcSunriseUtc	
        //* Type: Function	
        //* Purpose: calculate the Universal Coordinated Time (UTC) of sunrise	
        //*	 for the given day at the given location on earth	
        //* Arguments:	
        //* JD : julian day	
        //* latitude : latitude of observer in degrees	
        //* longitude : longitude of observer in degrees	
        //* Return value:	
        //* time in minutes from zero Z	
        //***********************************************************************/
        protected double CalcSunriseUtc (double JD, double latitudeVar, double longitudeVar) {
            double t = CalcTimeJulianCent (JD);

            // *** Find the time of solar noon at the location, and use
            // that declination. This is better than start of the 
            // Julian day

            double noonmin = CalcSolNoonUtc (t, longitudeVar);
            double tnoon = CalcTimeJulianCent (JD + noonmin / 1440.0);

            // *** First pass to approximate sunrise (using solar noon)

            double eqTime = CalcEquationOfTime (tnoon);
            double solarDec = CalcSunDeclination (tnoon);
            double hourAngle = CalcHourAngleSunrise (latitudeVar, solarDec);

            double delta = longitudeVar - hourAngle.ToDegrees ();
            double timeDiff = 4 * delta;	// in minutes of time
            double timeUTC = 720 + timeDiff - eqTime;	// in minutes

            // alert("eqTime = " + eqTime + "\nsolarDec = " + solarDec + "\ntimeUTC = " + timeUTC);

            // *** Second pass includes fractional jday in gamma calc

            double newt = CalcTimeJulianCent (CalcJDFromJulianCent (t) + timeUTC / 1440.0);
            eqTime = CalcEquationOfTime (newt);
            solarDec = CalcSunDeclination (newt);
            hourAngle = CalcHourAngleSunrise (latitudeVar, solarDec);
            delta = longitudeVar - hourAngle.ToDegrees ();
            timeDiff = 4 * delta;
            timeUTC = 720 + timeDiff - eqTime; // in minutes

            // alert("eqTime = " + eqTime + "\nsolarDec = " + solarDec + "\ntimeUTC = " + timeUTC);

            return timeUTC;
        }

        //***********************************************************************/
        //* Name: CalcSolNoonUtc	
        //* Type: Function	
        //* Purpose: calculate the Universal Coordinated Time (UTC) of solar	
        //*	 noon for the given day at the given location on earth	
        //* Arguments:	
        //* t : number of Julian centuries since J2000.0	
        //* longitude : longitude of observer in degrees	
        //* Return value:	
        //* time in minutes from zero Z	
        //***********************************************************************/
        protected double CalcSolNoonUtc (double t, double longitudeVar) {
            // First pass uses approximate solar noon to calculate eqtime
            double tnoon = CalcTimeJulianCent (CalcJDFromJulianCent (t) + longitudeVar / 360.0);
            double eqTime = CalcEquationOfTime (tnoon);
            double solNoonUTC = 720 + (longitudeVar * 4) - eqTime; // min

            double newt = CalcTimeJulianCent (CalcJDFromJulianCent (t) - 0.5 + solNoonUTC / 1440.0);

            eqTime = CalcEquationOfTime (newt);
            // double solarNoonDec = calcSunDeclination(newt);
            solNoonUTC = 720 + (longitudeVar * 4) - eqTime; // min

            return solNoonUTC;
        }

        //***********************************************************************/
        //* Name: CalcSunSetUtc	
        //* Type: Function	
        //* Purpose: calculate the Universal Coordinated Time (UTC) of sunset	
        //*	 for the given day at the given location on earth	
        //* Arguments:	
        //* JD : julian day	
        //* latitude : latitude of observer in degrees	
        //* longitude : longitude of observer in degrees	
        //* Return value:	
        //* time in minutes from zero Z	
        //***********************************************************************/
        protected double CalcSunSetUtc (double JD, double latitudeVar, double longitudeVar) {
            var t = CalcTimeJulianCent (JD);
            var eqTime = CalcEquationOfTime (t);
            var solarDec = CalcSunDeclination (t);
            var hourAngle = CalcHourAngleSunrise (latitudeVar, solarDec);
            hourAngle = -hourAngle;
            var delta = longitudeVar + hourAngle.ToDegrees ();
            var timeUTC = 720 - (4.0 * delta) - eqTime;	// in minutes
            return timeUTC;
        }

        //***********************************************************************/
        //* Name: CalcSunRiseUtc 
        //* Type: Function  
        //* Purpose: calculate the Universal Coordinated Time (UTC) of sunrise   
        //*  for the given day at the given location on earth   
        //* Arguments:  
        //* JD : julian day 
        //* latitude : latitude of observer in degrees  
        //* longitude : longitude of observer in degrees    
        //* Return value:   
        //* time in minutes from zero Z 
        //***********************************************************************/
        protected double CalcSunRiseUtc (double JD, double latitudeVar, double longitudeVar) {
            var t = CalcTimeJulianCent (JD);
            var eqTime = CalcEquationOfTime (t);
            var solarDec = CalcSunDeclination (t);
            var hourAngle = CalcHourAngleSunrise (latitudeVar, solarDec);
            var delta = longitudeVar + hourAngle.ToDegrees ();
            var timeUTC = 720 - (4.0 * delta) - eqTime;	// in minutes
            return timeUTC;
        }
    }
}

