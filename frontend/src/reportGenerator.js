import React, { useState, useEffect } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import './index.css';

const ReportGenerator = () => {
    const [reportUrl, setReportUrl] = useState('');
    const [processing, setProcessing] = useState(false);

    useEffect(() => {
        const apiUrl = process.env.REACT_APP_API_URL;
        const connection = new HubConnectionBuilder()
            .withUrl(`${apiUrl}/reportHub`)
            .withAutomaticReconnect()
            .build();

        connection.on('ReportReady', (url) => {
            setReportUrl(url);
            setProcessing(false); // When the report is ready the process is concluded
        });

        connection.start()
            .then(() => console.log('Connected to the SignalR hub'))
            .catch(err => console.error('Error connecting to SignalR hub:', err));

        return () => {
            connection.off('ReportReady');
            connection.stop();
        };
    }, []);

    const generateReport = async () => {
        const apiUrl = process.env.REACT_APP_API_URL;
        setProcessing(true); // Start report process
        const response = await fetch(`${apiUrl}/report/generate`, {
            method: 'POST'
        });

        if (response.ok) {
            console.log('Report generation started');
        } else {
            console.error('Failed to generate report');
            setProcessing(false); // If it fails, stop processing
        }
    };

    return (
        <div className="container">
            <div className="report-generator">
                <h1>SignalR with React</h1>
                <h2 className="title">Generate Report</h2>
                <div className="buttons">
                    <button className="generate-button" onClick={generateReport} disabled={processing}>
                        {processing ? 'Processing Report...' : 'Generate Report'}
                    </button>
                    {reportUrl && (
                        <a href={reportUrl} download target="_blank">
                            <button className="download-button">Download Report</button>
                        </a>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ReportGenerator;