import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { BusinessFlow } from './components/BusinessFlow';
import { Counter } from './components/Counter';
import { ServiceGrid } from './components/ServiceGrid';
import { RunSet } from './components/RunSet';

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/counter' component={Counter} />
            <Route path='/fetch-data' component={BusinessFlow} />
            <Route path='/ServiceGrid' component={ServiceGrid} />
            <Route path='/RunSet' component={RunSet} />
      </Layout>
    );
  }
}
