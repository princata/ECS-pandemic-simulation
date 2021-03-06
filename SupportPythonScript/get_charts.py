# -*- coding: utf-8 -*-
"""
Created on Tue Mar  1 12:16:11 2022

"""
# Importing packages
import os
import pandas as pd

import matplotlib.pyplot as plt
import numpy as np
from matplotlib.dates import DateFormatter, date2num
import dateutil.parser
import matplotlib.ticker as ticker
import seaborn as sns
sns.set()
from datetime import date
pd.options.mode.chained_assignment = None  # default='warn'

# Setting directories

os.chdir(r'C:/Users/step9/OneDrive/Desktop/Esperimenti/ScenarioRidotto/04-03-2022 simulations/NOpolicy30d_lockdown40d')  # Set your directory!
today = date.today()
# path = str(today)
# try:
#     os.mkdir(path)
# except OSError:
#     print ("Creation of the directory %s failed" % path)
# else:
#     print ("Successfully created the directory %s " % path)
# os.chdir(path)

doublePolicy = False
# Loading log file
# file = 'log.txt'
file = 'log.txt'

df = pd.read_csv(file, sep='\t')


# Generate a readable time variable - from minutes counter to day,min,sec
df['Time'] = pd.to_timedelta(df['MinutesPassed'], unit = 'm')

# Converting timedelta obkect in string as plt does not handle timedelta obj properly
df['Time str'] = df['Time'].astype(str)
df['Time str'] = df['Time str'].str[0:-3]

# # Ad hoc selection of timing for comparison 1,2
ind = df.index[df['Time str'].str[:2] ==  '41'].to_list()[0]
df = df.iloc[:ind,:]

# Daily database
# df['day'] = df['Time str'].str[:2].astype(int)
# list1 = list(df['day'].diff()[df['day'].diff() != 0].index.values)
# list2 = [x - 1 for x in list1]
# list2 = list2[1:]
# df_daily = df.iloc[list2, :]


cols = ['Exposed', 'ExposedVAX', 'TotalExposed',
       'Symptomatic', 'SymptomaticVAX', 'Asymptomatic', 'AsymptomaticVAX',
       'TotInfectedRetired', 'TotInfectedWorker', 'TotInfectedStudent',
       'Death', 'DeathVAX', 'TotDeathRetired', 'TotDeathWorker',
       'TotDeathStudent', 'Recovered', 'RecoveredVAX', 'TotalRecovered',
       'IntensiveCare', 'IntensiveCareVAX', 'TotalIntensive',
       'TotIntensiveRetired', 'TotIntensiveWorker', 'TotIntensiveStudent']

for c in cols:
    df[c + '_pp'] = (df[c]/df['Population'])*100
    
    #df_daily[c + '_diff'] = df_daily[c].diff()

# Chart 1: Cumulative counters over time
end_df = df.iloc[-1, :]
end_ex = round(end_df['Exposed_pp'], 2)
end_synt = round(end_df['Symptomatic_pp'], 2)
end_asym = round(end_df['Asymptomatic_pp'], 2)
end_death = round(end_df['Death_pp'], 2)
end_rec = round(end_df['Recovered_pp'], 2)
#VAX PERCENTAGES
end_exVAX = round(end_df['ExposedVAX_pp'], 2)
end_syntVAX = round(end_df['SymptomaticVAX_pp'], 2)
end_asymVAX = round(end_df['AsymptomaticVAX_pp'], 2)
end_deathVAX = round(end_df['DeathVAX_pp'], 2)
end_recVAX = round(end_df['RecoveredVAX_pp'], 2)
x = df['Time str']


start_date = df['Time str'].iloc[0]
end_date = df['Time str'].iloc[-1]


fig, ax = plt.subplots(figsize=(30, 25))
plt.ylabel('N of individuals', fontsize=40)
#plt.plot(x, df['TotalExposed'], label='TotalExposed')
plt.plot(x, df['Exposed'], label='Exposed',linewidth=4, color = 'y')
plt.plot(x, df['Symptomatic'], label='Symptomatics', linewidth=4, color = 'r')
plt.plot(x, df['Asymptomatic'], label='Asymptomatics',linewidth=4, color = 'm')
plt.plot(x, df['Death'], label='Deaths',linewidth=4, color = 'k')
plt.plot(x, df['Recovered'], label='Recovered',linewidth=4, color = 'g')
##VAX CURVES IF NEEDED
# =============================================================================
# plt.plot(x, df['ExposedVAX'], label='Exposed VAX',  alpha=0.5 ,linewidth=4, color = 'y')
# plt.plot(x, df['SymptomaticVAX'], label='Symptomatics VAX', alpha=0.5,  linewidth=4, color = 'r' )
# plt.plot(x, df['AsymptomaticVAX'], label='Asymptomatics VAX', alpha=0.5, linewidth=4, color = 'm')
# plt.plot(x, df['DeathVAX'], label='Deaths VAX',linewidth=4,  alpha=0.5, color = 'k')
# plt.plot(x, df['RecoveredVAX'], label='Recovered VAX',linewidth=4, alpha=0.5,  color = 'g')
# =============================================================================

#plt.title('Make your title')
ax.xaxis.set_major_locator(ticker.MaxNLocator(24))
ax.tick_params(axis='x', rotation=70)
legend = plt.legend(fontsize=40, bbox_to_anchor=(1.0, 1.01),handleheight =2 )
  
ax.tick_params(axis='both', which='major', labelsize=35)
x_vline = df.loc[df['ChangePolicies'] == 1, 'Time str'].iloc[0]  # get the date when lockdown is introduced
x_vline_str = x_vline[:7]
ax.axvline(x = x_vline, color = 'red')
locs, labels = plt.xticks()  # Get the current locations and labels.
ax.set_xlim(start_date, end_date)

#SCEGLIERE SE STAMPARE LE PERCENTUALI DEI VACCINATI
text = 'End of simulation counters, percentage of total population:\nExposed: ' + str(end_ex) + '%\nSymptomatics: ' + str(end_synt) + '%\nAsymptomatic: ' + str(end_asym) + \
        '%\nDeaths: ' + str(end_death) + '%\nRecovered: ' + str(end_rec) + '%'

# text = 'End of simulation counters, percentage of total population:\n' \
#            'Exposed: ' + str(end_ex) + '%              ExposedVAX: ' + str(end_exVAX) + '%' \
#            '\nSymptomatics: ' + str(end_synt) + '%     SymptomaticsVAX: ' + str(end_syntVAX) + '%' \
#            '\nAsymptomatic: ' + str(end_asym) + '%     AsymptomaticVAX: ' + str(end_asymVAX) + '%' \
#            '\nDeaths: ' + str(end_death) + '%                DeathsVAX: ' + str(end_deathVAX) + '%' \
#            '\nRecovered: ' + str(end_rec) + '%          RecoveredVAX: ' + str(end_recVAX) + '%'

if(doublePolicy):
    x_vline2 = df.loc[df['ChangePolicies'] == 1, 'Time str'].iloc[1]
    ax.axvline(x=x_vline2, color='red', linestyle ='-.')

# plt.figtext(5,0.01,'The graph shows cumulative numbers over the simulated horizon. The red vertical line represents the introduction of the lockdown after '
#             + x_vline_str + ' of simulation.'
#             , fontsize  =20)


# t = plt.annotate(text, xy=(1200, 2250),  xycoords='figure pixels', fontsize=20, bbox=dict(boxstyle="square,pad=0.3", fc="yellow", ec="b", lw=2))

#plt.show()

fig_name = 'NoPolicy30dLock10d' + str(today) 
fig.savefig(fig_name + '.png')


# # Chart 2: Daily counters over time
#
# x = df_daily['Time str']
# fig, ax = plt.subplots(figsize=(30,25))
# plt.plot(x,df_daily['Exposed_diff'], label = 'Exposed')
# plt.plot(x,df_daily['Symptomatic_diff'], label = 'Symptomatics')
# plt.plot(x,df_daily['Asymptomatic_diff'], label = 'Asymptomatics')
# plt.plot(x,df_daily['Death_diff'], label = 'Deaths')
# plt.plot(x,df_daily['Recovered_diff'], label = 'Recovered')
# #plt.title('Make your title')
# ax.xaxis.set_major_locator(ticker.MaxNLocator(12))
# ax.tick_params(axis='x', rotation=70)
# plt.legend(fontsize = 20)
# ax.tick_params(axis='both', which='major', labelsize=20)
# #x_vline_str = '30 days'
# #ax.axvline(x = x_vline, color = 'red')
# #plt.show()
# fig.savefig('Daily counters over time.png')
