int compareValues(double[][] receivedData, double[][] databaseData){
	/* This function assumes that receivedData and databaseData have already been run through normalizeAndInterpolateTimes()
		as well as averageOutValues() */
	/* We first attempt to estimate the offset of the data, by differencing the average values of receivedData and databaseData
		(averaged over number of samples, not time). 
	 	We then allow each data point to have some leeway
	 	and finally we account for outliers by having a threshold of failures, dependant again on the number of samples. */

	int[3] difference = {0,0,0,0};
	int receivedLength = receivedData[0].length();
	int databaseLength = databaseData[0].length();
	int asdf = (receivedLength < databaseLength ? databaseLength/10000 : receivedLength/10000);
	int[3] failureThreshold = {asdf, asdf, asdf, asdf};


	//OFFSET VALUES:
	int[3] receivedAverage = {0,0,0,0};
	int[3] databaseAverage = {0,0,0,0};
	for (int i = 0; i <= receivedLength; ++i){
		for (int j = 0; j < 4; ++j){
			receivedAverage[j] += receivedData[j+1][i];
		}
	} for (int i = 0; i <= databaselength ++i){
		for (int j = 0; j < 4; ++j){
			databaseAverage[j] += databaseData[j+1][i];
		}
	}
	for (int j = 0; j < 4; ++j){
		receivedAverage[j] /= receivedLength[j];
		databaseAverage[j] /= databaseLength[j];
		difference[j] = databaseAverage[j] - receivedAverage[j];
	}

	//LEEWAY:
	int j_index = 0;
	double timingleeway = 0.01;
	double[3] leeway = {80, 30, 30, 30}; //MESS WITH THESE VALUES
	for (int i = 0; i <= receivedLength; ++i){
		for (int j = j_index; j < j_index + 100; ++j){
			if (j > databaseLength){
				break;
			}
			if (isWithin(databaseData[0][j], receivedData[0][i], timingleeway)){
				j_index = j;
				break;
			}
		}
		for (int k = 0; k < 4; ++k){
			if (!isWithin(databaseData[k][j_index], receivedData[k][i]), leeway){		//Iffy on the logic of this line
				--failureThreshold[k];
				if (failureThreshold[k] == 0){
					return 100; //FAILED!
				}
			}
		}
	}

	return 0;
}

static bool isWithin(double one, double two, double threshold, bool signed = false){
	double diff = one - two;
	if (signed == false){
		diff = (diff < 0 ? -1*diff : diff);	
	}
	if (diff < threshold){
		return true;
	} else {
		return false;
	}
}


static double[][] averageOutValues(double[][] data){
	/* adds up the 50 values surrounding each data point, averages the value,
		and stores it back in the data point. Gets rid of rapid jumps */

	int interval = 51; //The number of indices over which to average out values (keep as an ODD value)
	int datalength = data[0].length();

	for (int i = 0; i < datalength; ++i){
		double sumValueOne = 0;
		double sumValueTwo = 0;
		double sumValueThree = 0;
		double sumValueFour = 0;

		for (int j = i - interval/2; j <= i + interval/2; ++j){
			if (j < 0 || j > datalength){
				continue;
			}
			sumValueOne += data[1][j];
			sumValueTwo += data[2][j];
			sumValueThree += data[3][j];
			sumValueFour += data[4][j];
		}

		data[1][i] = sumValueOne / 51.0;
		data[2][i] = sumValueTwo / 51.0;
		data[3][i] = sumValueThree / 51.0;
		data[4][i] = sumValueFour / 51.0;
	}

	return data;
}



static double[][] normalizeAndInterpolateTimes(double[][] data){
	int normalizeTimes = data[0][0];
	int currTime = 0;
	int nextTime = 0;

	//Subtract the first timestamp so the times are always in a similar range:
	for (int i = 0; i < data[0].length(); ++i){	data[0][i] -= normalizeTimes;	}
	
	int previousFloorTime = data[0][0];
	int previousFloorIndex = 0;

	int nextFloorTime = 0;
	int nextFloorIndex = 0;

	for (int i = 0; i < data[0].length(), ++i){
		nextTime = data[0][i+1];
		currTime = data[0][i];

		if (nextTime > currTime){
			nextFloorIndex = i+1;
			nextFloorTime = data[0][i+1];
			for (int j = previousFloorIndex + 1; j < nextFloorIndex - previousFloorIndex; ++j){
				//To time, add the value of: slope between the 'corners' of the times * number of data points between this data point and the previous 'corner' time's data point.
				data[0][j] += (nextFloorTime - previousFloorTime)/(nextFloorIndex - previousFLoorIndex) * (j - previousFloorIndex);
			}

			previousFloorIndex = nextFloorIndex;
			previousFloorTime = nextFloorTime;
		}
	}
	return data;
}