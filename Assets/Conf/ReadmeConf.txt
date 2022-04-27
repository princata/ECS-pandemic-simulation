Briefly explaination of the parameteres within the "conf.txt" file.

"appendLog" -> bool: allows the user to start the new simulation from the status
quo reached in the previously launched simulation. Practically, new counters are
appended to the log already saved.

"heatmap" -> bool: allows the user to set a bigger size of agent's representation in the map, in order
to better visualize virus spreading process.

"numberOfHumans" -> int: the number of the humans at the beginning of the simulation.

"numberOfInfects" -> int: the number of symptomatic people at the beginning.

"timeScale" -> float: the speed of the simulation.

"minDaysInfectious" -> int: lower threshold of days of Covid-19 infectiousness.

"maxDaysInfectious" -> int: upper threshold of days of Covid-19 infectiousness. 

"minDaysRecovered" -> int: lower threshold of days of recovering from the virus.  

"maxDaysRecovered" -> int: upper threshold of days of recovering from the virus.   

"minDaysExposed" -> int: lower threshold of days of incubation period.

"maxDaysExposed" -> int: upper threshold of days of incubation period. 

"map" -> string: the name of the map file.

"lockdown" -> bool: general lockdown.

"vaccinationPolicy" -> bool: it activates the vaccination policy.

"lockGym" -> bool: it activates lockdown at gyms.

"lockSchool" -> bool: it activates lockdown at schools.

"lockPubs" -> bool: it activates lockdown at pubs/restaurants.

"malePercentage" -> float: percentage of male population in the simulation.

"inputAge" -> int array: age distribution of agents in the simulation.

"randomSocResp" -> bool: allows the user to randomize the 'Social Responsibility' parameter for each agent.

"socialResponsibility" -> float: if the previous parameter is false, this one is considered and assign the indicated
value to each agent.

"noVaxPercentage" -> float: percentage of people who will not get vaccinated in the simulation,
if the 'vaccinationPolicy' parameter is activated.

"icu4100k" -> int: the number of 'Intensive Care Unit' every 100,000 inhabitants.

"symptomsStudent" -> float: probability of developing symptoms for a student agent.

"ifrStudent" -> float: literally 'Infection Fatalaty Rate' for student agent, it indicates the probability
of dying once infected by the virus.

"symptomsWorker" -> float: probability of developing symptoms for a worker agent.

"ifrWorker" -> float: literally 'Infection Fatalaty Rate' for worker agent, it indicates the probability
of dying once infected by the virus.

"symptomsRetired" -> float: probability of developing symptoms for a retired agent.

"ifrRetired" -> float: literally 'Infection Fatalaty Rate' for retired agent, it indicates the probability
of dying once infected by the virus.

"minDaysFDTstudent" -> int: lower treshold of days to adminstrate the first dose to student agents.

"maxDaysFDTstudent" -> int: upper treshold of days to adminstrate the first dose student agents.

"minDaysFDTworker" -> int: lower treshold of days to adminstrate the first dose worker agents.

"maxDaysFDTworker" -> int: upper treshold of days to adminstrate the first dose worker agents.

"minDaysFDTretired" -> int: lower treshold of days to adminstrate the first dose retired agents.

"maxDaysFDTretired" -> int: upper treshold of days to adminstrate the first dose retired agents.

"exposureTime" -> float: minimum period of time that must pass to be infected, expressed in minutes.

"contagionDistance" -> float: mininum distance to be infected, expressed in meters.

"protectionRecovered" -> float: protection from disease after recovering from it, expressed in percentage.

"protectionVaccinated"-> float: protection from disease after getting vaccinated, expressed in percentage.

"protectionAfterImmunity" -> float: protection after 'daysOfImmunity', expressed in percentage. 
Then protection will deacrease slowly in time.

"householdTRs" -> float: virus transmission rate for symptomatic people in household, expressed in percentage.
This parameter helps in reaching earlier the 'exposureTime'.

"householdTRa" -> float: virus transmission rate for asymptomatic people in household, expressed in percentage.
This parameter helps in reaching earlier the 'exposureTime'.

"workplaceTRs" -> float: virus transmission rate for symptomatic people in workplaces, expressed in percentage.
This parameter helps in reaching earlier the 'exposureTime'.

"workplaceTRa" -> float: virus transmission rate for asymptomatic people in workplaces, expressed in percentage.
This parameter helps in reaching earlier the 'exposureTime'.

"retirehouseTRs" -> float: virus transmission rate for symptomatic people in retirement home, expressed in percentage.
This parameter helps in reaching earlier the 'exposureTime'.

"retirehouseTRa" -> float: virus transmission rate for asymptomatic people in retirement home, expressed in percentage.
This parameter helps in reaching earlier the 'exposureTime'.

"outdoorTRs" -> float: virus transmission rate for symptomatic people in outdoor places, expressed in percentage.
This parameter helps in reaching earlier the 'exposureTime'.

"outdoorTRa" -> float: virus transmission rate for asymptomatic people in outdoor places, expressed in percentage.
This parameter helps in reaching earlier the 'exposureTime'.

"indoorTRs" -> float: virus transmission rate for symptomatic people in remaining indoor places, expressed in percentage.
This parameter helps in reaching earlier the 'exposureTime'.

"indoorTRa" -> float: virus transmission rate for asymptomatic people in remaining indoor places, expressed in percentage.
This parameter helps in reaching earlier the 'exposureTime'.

"schoolTRs" -> float: virus transmission rate for symptomatic people in schools, expressed in percentage.
This parameter helps in reaching earlier the 'exposureTime'.

"schoolTRa" -> float: virus transmission rate for asymptomatic people in schools, expressed in percentage.
This parameter helps in reaching earlier the 'exposureTime'.

"eatingOutProb" -> float: probability to eat outside instead of staying at home, expressed in percentage.

"visitFriendProbNL" -> float: probability to visit friend for sociability in case of 'lockdown' disactivated, expressed in percentage.

"visitFriendProbL" -> float: probability to visit friend for sociability in case of 'lockdown' activated, expressed in percentage.

"remoteWorkerPercent" -> float: percentage of remote worker across the population. Remote work is considered only if 'lockdown' policy is acrivated.

"maxDoses" -> int: the maximum number of doses that an agent can get during the simulation. 
This value is considered only if 'vaccination policy is activated.

"daysBTWdoses" -> int: the number of days that must pass between two consecutive vaccine doses.
This value is considered only if 'vaccination policy is activated.

"daysOfImmunity" -> int: the number of days that must pass to lose disease protection up to 'protectionAfterImmunity'.

"hungerOnset" -> float: how many hours must pass to be hungry again.

"hungerDuration" -> float: how many hours the agent eats whenever he is hungry.

"restDuration" -> float: how many hours the agent sleeps during the day.

"sociabilityOnset" -> float: how many hours must pass to engage in social activities again.

"sociabilityDuration" -> float: how many hours the agent needs to satisfy his sociability.

"sportmanshipOnset" -> float: how many hours must pass to engage in sport activities again.

"sportmanshipDuration" -> float: how many hours the agent needs to satisfy his sportmanship.

"groceryOnset" -> float: how often the agent go to the supermarket, expressed in hours.

"groceryDuration" -> float: how much time the agent spend at the supermarket, expressed in hours.

"workDuration" -> float: how many hours the agent works during the day.

"familyTemplate" -> int array: definition of the possible family template in the simulation.
Legend: 1 -> student
        2 -> worker
	3 -> retired
Example: [221,33,2] -> there are three possible templates: 
two worker and one student,
two retired,
one worker
							
"familyDistrib" -> float array: family pattern distribution percentage on total population.
The order of the elements in this array must be same of previous one, in order to associate 
the right percentage to the right family pattern.


