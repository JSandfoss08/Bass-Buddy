import matplotlib.pyplot as plt
import pandas as pd
import numpy as np
import sys
import os

# If you guys experience any issues with dependencies, you should be able to do the following:

# For Windows
# Navigate in the cmdline to Assets/DataPipeline
# Run: "venv\Scripts\activate"
# Run: "pip install -r requirements.txt"

# For Mac/Linux
# Navigate in the cmdline to Assets/DataPipeline
# Run: "venv/bin/activate"
# Run: "pip install -r requirements.txt"

baseLogDir = sys.argv[1]
cleanedLogsPath = os.path.join(baseLogDir, "Processed_Data")
# This can be our directory for all cleaned data
os.makedirs(cleanedLogsPath, exist_ok=True)

def ProcessDatabaseData():
    databaseAddUserFile = os.path.join(baseLogDir, "AddUser_UnitTest.csv")
    databaseRemoveUserFile = os.path.join(baseLogDir, "RemoveUser_UnitTest.csv")
    databaseUpdateLessonFile = os.path.join(baseLogDir, "UpdateLessonProgress_UnitTest.csv")
    databaseIntegrationAddRemoveFile = os.path.join(baseLogDir, "AddRemove_IntegrationTest.csv")
    databaseIntegrationAddUpdateRemoveFile = os.path.join(baseLogDir, "AddUpdateRemove_IntegrationTest.csv")

    databaseCleanedDataFile = os.path.join(cleanedLogsPath, "DB_Data_Log_Cleaned.csv")

    test_files = {
        "AddUser": databaseAddUserFile,
        "RemoveUser": databaseRemoveUserFile,
        "UpdateLesson": databaseUpdateLessonFile,
        "IntegrationAddRemove": databaseIntegrationAddRemoveFile,
        "IntegrationAddUpdateRemove": databaseIntegrationAddUpdateRemoveFile
    }

    dfs = []

    for testName, path in test_files.items():
        df = pd.read_csv(path)
        df['TestType'] = testName
        dfs.append(df)
        PlotIndividualTest(df, testName)

    allData = pd.concat(dfs, ignore_index=True)
    allData["NormalizedResponse"] = (allData["ResponseTime(ms)"] / allData["ResponseTime(ms)"].max())
    allData.to_csv(databaseCleanedDataFile, index=False)

    PlotCombined(allData)

def PlotIndividualTest(df, testName, referenceRequirement=None):
    df['IsPass'] = df['Result'] == "PASS"

    plt.figure()
    plt.title(f"{testName}: Response Time Per Iteration")
    plt.xlabel("Test Iteration")
    plt.ylabel("Response Time in milliseconds")
    plt.plot(df.index, df['ResponseTime(ms)'])
    if referenceRequirement != None:
        plt.axhline(referenceRequirement, linestyle='--')
    plt.savefig(os.path.join(cleanedLogsPath, f"{testName}_ResponseTimePlot.png"))
    plt.close()

def PlotCombined(allData):
    # Response time plot
    plt.figure()
    plt.title("All Database Test Response Times")
    plt.xlabel("Entry Index")
    plt.ylabel("Response Time in milliseconds")

    for testType in allData['TestType'].unique():
        subset = allData[allData['TestType'] == testType]
        plt.plot(subset.index, subset['ResponseTime(ms)'], label=testType)

    plt.legend()
    plt.savefig(os.path.join(cleanedLogsPath, "DB_AllTests_ResponseTimePlot.png"))
    plt.close()

    # Accuracy plot
    plt.figure()
    plt.title("All Database Tests – Pass Rate Per Test Group")
    plt.xlabel("Test Type")
    plt.ylabel("Pass Rate (%)")

    pass_rates = (
        allData.groupby("TestType")["Result"]
        .apply(lambda x: (x == "PASS").mean() * 100)
    )

    plt.bar(pass_rates.index, pass_rates.values)
    plt.savefig(os.path.join(cleanedLogsPath, "DB_AllTests_PassRate.png"))
    plt.close()

def ProcessAudioData():
    audioInputLogFile = os.path.join(baseLogDir, "Pitch_to_Note.csv")
    pitchCleanedDataFile = os.path.join(cleanedLogsPath, "Pitch_to_Note_Cleaned.csv")

    note_freqs = {
        "C":    [16.35, 32.70, 65.41, 130.81, 261.63, 523.25, 1046.50, 2093.00, 4186.01],
        "C#":   [17.32, 34.65, 69.30, 138.59, 277.18, 554.37, 1108.73, 2217.46, 4434.92],
        "D":    [18.35, 36.71, 73.42, 146.83, 293.66, 587.33, 1174.66, 2349.32, 4698.63],
        "D#":   [19.45, 38.89, 77.78, 155.56, 311.13, 622.25, 1244.51, 2489.02, 4978.03],
        "E":    [20.60, 41.20, 82.41, 164.81, 329.63, 659.25, 1318.51, 2637.02, 5274.04],
        "F":    [21.83, 43.65, 87.31, 174.61, 349.23, 698.46, 1396.91, 2793.83, 5587.65],
        "F#":   [23.12, 46.25, 92.50, 185.00, 369.99, 739.99, 1479.98, 2959.96, 5919.91],
        "G":    [24.50, 49.00, 98.00, 196.00, 392.00, 783.99, 1567.98, 3135.96, 6271.93],
        "G#":   [25.96, 51.91, 103.83, 207.65, 415.30, 830.61, 1661.22, 3322.44, 6644.88],
        "A":    [27.50, 55.00, 110.00, 220.00, 440.00, 880.00, 1760.00, 3520.00, 7040.00],
        "A#":   [29.14, 58.27, 116.54, 233.08, 466.16, 932.33, 1864.66, 3729.31, 7458.62],
        "B":    [30.87, 61.74, 123.47, 246.94, 493.88, 987.77, 1975.53, 3951.07, 7902.13]
    }

    pitchData = pd.read_csv(audioInputLogFile)
    pitchData = pitchData.dropna()
    pitchData['DetectedPitch'] = pitchData['DetectedPitch'].astype(float)

    def getError(detectedPitch, truePitch):
        if detectedPitch == 0 or truePitch == 0:
            print("detectedPitch is 0 or true pitch is 0")
            return None
        return 1200 * (np.log2(detectedPitch / truePitch))

    pitchData['CentsError'] = np.nan
    pitchData['PitchDifference'] = np.nan
    pitchData['PercentError'] = np.nan
    pitchData['Within5Percent'] = False

    for index, row in pitchData.iterrows():
        detectedNote = row['NoteName']
        pitch = float(row['DetectedPitch'])
        truePitch = min(note_freqs[detectedNote], key=lambda f: abs(f - pitch))

        pitchData.at[index, 'PitchDifference'] = truePitch - pitch
        pitchData.at[index, 'CentsError'] = 1200 * (np.log2(pitch / truePitch))

        percent_error = abs((pitch - truePitch) / truePitch) * 100
        pitchData.at[index, 'PercentError'] = percent_error
        pitchData.at[index, 'Within5Percent'] = percent_error <= 5

    pitchData['Correctness'] = pitchData['CentsError'].abs() < 50

    pitchData.to_csv(pitchCleanedDataFile, index=False)

    plt.figure()
    plt.title("Pitch Detection Percent Error")
    plt.xlabel("Sample Index")
    plt.ylabel("Percent Error")
    plt.scatter(pitchData.index, pitchData["PercentError"])
    plt.axhline(5, linestyle='--')
    plt.savefig(os.path.join(cleanedLogsPath, "PitchAccuracyPlot.png"))
    plt.close()

def main():
    ProcessAudioData()
    ProcessDatabaseData()

if __name__ == "__main__":
    main()
