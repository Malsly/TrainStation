﻿using System;
using System.Collections.Generic;
using Gtk;
using TrainStation;

public partial class MainWindow : Gtk.Window
{
    static List<Train> TrainList = new List<Train>();
    static List<Passanger> PassangerList = new List<Passanger>();
    Station station = new Station(TrainList, PassangerList);


    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Dictionary<string, int> RouteAndPriceKievChernigiv = new Dictionary<string, int>
        {
            { "Kiev", 0 },
            { "Kozelets", 30 },
            { "Desna", 60},
            { "Chernigiv", 100 }
        };

        Dictionary<string, DateTime> RouteAndDateKievChernigiv = new Dictionary<string, DateTime>
        {
            { "Kiev", DateTime.Now  },
            { "Kozelets", DateTime.Now + new TimeSpan(0, 3, 0, 0) },
            { "Desna", DateTime.Now + new TimeSpan(0, 4, 0, 0) },
            { "Chernigiv", DateTime.Now + new TimeSpan(1, 0, 0, 0) }
        };

        Dictionary<string, int> RouteAndPriceKievLugansk = new Dictionary<string, int>
        {
            { "Kiev", 0 },
            { "Kozelets", 30 },
            { "Charkiv", 45 },
            { "Zhitomir", 75 },
            { "Lugansk", 130 }
        };

        Dictionary<string, DateTime> RouteAndDateKievLugansk = new Dictionary<string, DateTime>
        {
            { "Kiev", DateTime.Now  },
            { "Kozelets", DateTime.Now + new TimeSpan(0, 3, 0, 0) },
            { "Charkiv", DateTime.Now + new TimeSpan(1, 0, 0, 0) },
            { "Zhitomir", DateTime.Now + new TimeSpan(1, 5, 0, 0) },
            { "Lugansk", DateTime.Now + new TimeSpan(1, 13, 0, 0) }
        };

        Dictionary<string, int> RouteAndPriceLvivKiev = new Dictionary<string, int>
        {
            { "Lviv", 0 },
            { "Gorodishe", 90 },
            { "Donetsk", 145 },
            { "Pomoshna", 275 },
            { "Kiev", 330 }
        };

        Dictionary<string, DateTime> RouteAndDateLvivKiev = new Dictionary<string, DateTime>
        {
            { "Lviv", DateTime.Now },
            { "Gorodishe", DateTime.Now + new TimeSpan(0, 3, 0, 0) },
            { "Donetsk", DateTime.Now + new TimeSpan(0, 8, 0, 0) },
            { "Pomoshna", DateTime.Now + new TimeSpan(0, 16, 0, 0) },
            { "Kiev", DateTime.Now + new TimeSpan(1, 3, 0, 0) }
        };

        station.AddTrain("Kiev", "Chernigiv", RouteAndDateKievChernigiv, RouteAndPriceKievChernigiv, new List<Van>());
        station.AddTrain("Kiev", "Lugansk", RouteAndDateKievLugansk, RouteAndPriceKievLugansk, new List<Van>());
        station.AddTrain("Lviv", "Kiev", RouteAndDateLvivKiev, RouteAndPriceLvivKiev, new List<Van>());

        foreach(Train train in station.TrainList) 
        {
            train.CreateVansForTrain(10, "Plackart");
            train.CreateVansForTrain(2, "Cupe");
            foreach(Van van in train.VanList) 
            {
                van.CreateSeatForVan(15, "Main");
                van.CreateSeatForVan(15, "Side");
            }
        }

        Van.AddClassAndPrice("Plackart", 0);
        Van.AddClassAndPrice("Cupe", 20);

        Seat.AddTypeAndPrice("Main", 0);
        Seat.AddTypeAndPrice("Side", 0);

        Deb.Print(station.TrainList[0].RouteAndDate);

        Build();
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    protected void OnRegestrationClicked(object sender, EventArgs e)
    {
        string name = this.NamePassanger.Text;
        string telephone = this.Telephone.Text;
        string email = this.Email.Text;

        if (Passanger.IsValidUser(name, telephone, email)) 
        {
            Passanger passanger = station.RegestrationPassanger(name, telephone, email);
            this.Place.Show();
            this.PlaceTextView.Show();
            this.ChooseTrainBtn.Show();
        }
        else {
            this.TextAttention.Buffer.Text = "Not valid user";
            this.Place.Hide();
            this.PlaceTextView.Hide();
            this.ChooseTrainBtn.Hide();
        }


        Deb.Print(station.TrainList);
        Deb.Print(station.PassangerList);
    }

    protected void OnChooseTrainClicked(object sender, EventArgs e)
    {
        ListStore DataSourceForTrains = new ListStore(typeof(string), typeof(Train));
        this.TrainsListComboBox.Model = DataSourceForTrains;

        string placeDepartureText = this.placeDeparture.Text;
        string placeArrivalText = this.placeArrival.Text;

        if (placeArrivalText != "" && placeDepartureText != "")
        {
            this.TrainsListComboBox.Show();
            foreach(Train train in station.TrainList) 
            {
                if(train.PlaceDeparture.Contains(placeDepartureText) && train.RouteAndPrice.ContainsKey(placeArrivalText)) 
                {
                    if (train.PlaceArrival == placeArrivalText) 
                    {
                        DataSourceForTrains.AppendValues($"{train.PlaceDeparture} - {train.PlaceArrival}", train);
                    }
                    else { 
                        DataSourceForTrains.AppendValues($"{train.PlaceDeparture} - {placeArrivalText} - {train.PlaceArrival}", train);

                    } 
                }
            }
        }
        else
        {
            this.TrainsListComboBox.Hide();
        }
    }

    protected void OnTrainsListComboBoxChanged(object sender, EventArgs e)
    {
        Dictionary<string, Train> RouteAndTrain = new Dictionary<string, Train>();
        RouteAndTrain.Clear();
        this.ChoosVanComboBox.Show();

        TreeIter iter;
        if (this.TrainsListComboBox.Model.GetIterFirst(out iter))
        {
            do
            {
                RouteAndTrain.Add((string)this.TrainsListComboBox.Model.GetValue(iter, 0), (Train)this.TrainsListComboBox.Model.GetValue(iter, 1));
            } while (this.TrainsListComboBox.Model.IterNext(ref iter));
        }
        Train choosedTrain;
        RouteAndTrain.TryGetValue(this.TrainsListComboBox.ActiveText, out choosedTrain);


        ListStore DataSourceForVans = new ListStore(typeof(int), typeof(Van));
        this.ChoosVanComboBox.Model = DataSourceForVans;

        foreach(Van van in choosedTrain.VanList) 
        {
            DataSourceForVans.AppendValues(van.Number, van); 
        }

    }

    protected void OnChoosVanComboBoxChanged(object sender, EventArgs e)
    {
        Dictionary<int, Van> NumberAndVan = new Dictionary<int, Van>();

        TreeIter iter;
        if (this.ChoosVanComboBox.Model.GetIterFirst(out iter))
        {
            do
            {
                NumberAndVan.Add((int)this.ChoosVanComboBox.Model.GetValue(iter, 0), (Van)this.ChoosVanComboBox.Model.GetValue(iter, 1));
            } while (this.ChoosVanComboBox.Model.IterNext(ref iter));
        }

        Van choosedVan;

        NumberAndVan.TryGetValue(Convert.ToInt32(this.ChoosVanComboBox.ActiveText), out choosedVan);

        Deb.Print(choosedVan.Class);

        this.ChooseSeatComboBox.Show();
    }

    protected void OnChooseSeatComboBoxChanged(object sender, EventArgs e)
    {
        string choosedSeat = this.ChooseSeatComboBox.ActiveText;
        this.TicketingBtn.Show();
    }
}
