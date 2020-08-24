import { Component, h } from "preact";
import { Meme } from "../../models";
import ky from "ky";
import { route } from "preact-router";
import Loading from "../../components/Loading";


interface State {
    meme?: Meme;
}

export default class LandingPage extends Component<any, State> {
    componentDidMount() {
        ky.get(`/api/meme`)
            .json<Meme>()
            .then(meme => route(`/${meme.threadId}/${meme.id}`));
    }

    render(){
        return (
            <Loading/>
        );
    }
}
