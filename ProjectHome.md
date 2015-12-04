<font color='blue' face='Times New Roman;Courier New' size='8'>HFM.NET</font>

<font face='Times New Roman;Courier New' size='4'><i>Folding@Home Client Monitoring Application.</i></font>

<font color='blue' face='Times New Roman;Courier New' size='3'>Donate to the HFM.NET Project with PayPal.</font>

[![](https://www.paypal.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=5759628)

<br>

<font face='Times New Roman;Courier New' size='6'><a href='https://drive.google.com/open?id=0B8d5F59S5sCiS1RISzdsaEd5UXM&authuser=0'>New Download Location on Google Drive</a></font>

<br>

<h1>Change Log - Version 0.9.3 - <a href='https://code.google.com/p/hfm-net/source/detail?r=718'>Revision 718</a></h1>

<ul><li>Release Date: February 18, 2015</li></ul>

<ul><li>Fix: Make HFM compatible with old and new psummary pages.</li></ul>


<h1>Change Log - Version 0.9.2 - <a href='https://code.google.com/p/hfm-net/source/detail?r=712'>Revision 712</a></h1>

<ul><li>Release Date: September 3, 2014</li></ul>

<ul><li>Fix: HFM now supports FAH v7 Client versions 7.1.52 to 7.4.4.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=287'>Issue 287</a> - Problem with mixed UTC and non-UTC date & time values being written to and read from SQLite.<br>
</li><li>Fix: Work Unit History database upgrade - remove duplicate entries and update the populate the work unit information based on current psummary data.<br>
</li><li>Fix: Mono fixes and updates from tear and Team 33.<br>
</li><li>Fix: Mono verison checking logic for Mono 3.x from jimerickso.<br>
</li><li>Fix: Add detection for GRO_A? style core names.<br>
</li><li>Fix: Add detection for ZETA cores.<br>
</li><li>Fix: Handle bigbeta client type from v7 clients correctly.</li></ul>

<ul><li>Enhancement: Add NOT EQUAL, LIKE, and NOT LIKE operators for work unit history queries.</li></ul>

<ul><li>Change: Remove the distinction of Uniprocessor or SMP client.  All cpu based clients/slots are now shown simply as CPU.<br>
</li><li>Change: Add work unit data to work unit history table.  This change supports capturing the work unit parameters (points, kfactor, etc) in the database.  Doing so allows HFM to display PPD and credit information for the work unit as the work unit was valued at the point in time the work unit was completed.<br>
</li><li>Change: Add detection of client start date/time for legacy (v6) clients based on date present in log file opening.<br>
</li><li>Change: Add detection of client start date/time for v7 clients based on date present in log file opening.<br>
</li><li>Change: v6 and v7 clients/slots now source the Completed and Failed counts from the WU History database.</li></ul>


Please see the <a href='VersionHistory.md'>VersionHistory</a> Page for change log history.